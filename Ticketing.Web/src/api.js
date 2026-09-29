const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '/api/v1').replace(/\/$/, '');

export class ApiError extends Error {
  constructor(message, status, details) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? 'GET',
    headers: {
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...(options.headers ?? {})
    },
    body: options.body ? JSON.stringify(options.body) : undefined
  });

  const text = await response.text();
  let payload = null;

  if (text) {
    try {
      payload = JSON.parse(text);
    } catch {
      payload = text;
    }
  }

  if (!response.ok) {
    throw new ApiError(getErrorMessage(payload, response.status, response.statusText), response.status, payload);
  }

  return payload;
}

function getErrorMessage(payload, status, fallback) {
  if (!payload) {
    return `Request failed with status ${status}${fallback ? ` (${fallback})` : ''}`;
  }

  if (typeof payload === 'string') {
    return payload;
  }

  if (payload.detail) {
    return payload.detail;
  }

  if (payload.title) {
    return payload.title;
  }

  if (payload.errors && typeof payload.errors === 'object') {
    return Object.entries(payload.errors)
      .map(([key, value]) => `${key}: ${Array.isArray(value) ? value.join(', ') : value}`)
      .join(' | ');
  }

  return `Request failed with status ${status}`;
}

export const api = {
  getEvents(pageNumber = 1, pageSize = 20) {
    return request(`/events?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  },
  createEvent(payload) {
    return request('/events', { method: 'POST', body: payload });
  },
  updateEvent(eventId, payload) {
    return request(`/events/${eventId}`, { method: 'PUT', body: payload });
  },
  deleteEvent(eventId) {
    return request(`/events/${eventId}`, { method: 'DELETE' });
  },
  getAvailability(eventId) {
    return request(`/tickets/availability/${eventId}`);
  },
  purchaseTickets(payload) {
    return request('/tickets/purchases', { method: 'POST', body: payload });
  },
  getPurchaseById(purchaseId) {
    return request(`/tickets/purchases/${purchaseId}`);
  },
  getPurchaseByReference(bookingReference) {
    return request(`/tickets/purchases/reference/${bookingReference}`);
  },
  getAllSalesSummaries() {
    return request('/reports/events/sales-summary');
  },
  getEventSalesSummary(eventId) {
    return request(`/reports/events/${eventId}/sales-summary`);
  }
};


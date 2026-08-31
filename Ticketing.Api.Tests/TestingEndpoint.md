## Ticketing API testing scenario

Use any of the following event IDs in the requests below:

| ID | Name |
|:--|:--|
| `73060548-fdc3-4b0b-b8f8-ff73fad81de2` | Jazz Under Stars |
| `8c746243-fd4f-424c-83f1-89453063ebc6` | Nordic Lights Live |
| `b7c0ec01-b6f2-4b63-b275-f376d51dc6da` | Symphonic Evenings |

### 1) Check ticket availability

```bash
curl -X GET "http://localhost:5000/api/v1/tickets/availability/73060548-fdc3-4b0b-b8f8-ff73fad81de2"
```

Verify the response contains:

- `totalCapacity`
- `totalTicketsSold`
- `totalTicketsAvailable`
- `availabilityByTier`

### 2) Purchase tickets

```bash
curl -X POST "http://localhost:5000/api/v1/tickets/purchases" \
  -H "Content-Type: application/json" \
  -d '{
	"eventId": "73060548-fdc3-4b0b-b8f8-ff73fad81de2",
	"pricingTierId": "<pricing-tier-id-from-event-details>",
	"customerName": "Alice",
	"customerEmail": "alice@example.com",
	"quantity": 2
  }'
```

The response returns a `bookingReference` value. Save it for the next request.

### 3) Get booking details by booking reference

```bash
curl -X GET "http://localhost:5000/api/v1/tickets/purchases/reference/<bookingReference>"
```

This should return the same purchase details as the POST response.

### 4) Check availability again

```bash
curl -X GET "http://localhost:5000/api/v1/tickets/availability/73060548-fdc3-4b0b-b8f8-ff73fad81de2"
```

Confirm the sold and available counts have changed after the purchase.

### 5) Run sales summary report

```bash
curl -X GET "http://localhost:5000/api/v1/reports/events/73060548-fdc3-4b0b-b8f8-ff73fad81de2/sales-summary"
```

Verify the report includes:

- `totalTicketsSold`
- `remainingTickets`
- `totalRevenue`
- `salesByTier`

### Controller routes used

- `GET /api/v1/tickets/availability/{eventId}`
- `POST /api/v1/tickets/purchases`
- `GET /api/v1/tickets/purchases/reference/{bookingReference}`
- `GET /api/v1/reports/events/{eventId}/sales-summary`

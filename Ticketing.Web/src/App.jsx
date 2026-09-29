import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  AppBar,
  Box,
  Button,
  Chip,
  CircularProgress,
  Collapse,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  MenuItem,
  Pagination,
  Paper,
  Snackbar,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Toolbar,
  Typography
} from '@mui/material';
import AddCircleOutlineRoundedIcon from '@mui/icons-material/AddCircleOutlineRounded';
import ConfirmationNumberRoundedIcon from '@mui/icons-material/ConfirmationNumberRounded';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import EventRoundedIcon from '@mui/icons-material/EventRounded';
import ExpandLessRoundedIcon from '@mui/icons-material/ExpandLessRounded';
import ExpandMoreRoundedIcon from '@mui/icons-material/ExpandMoreRounded';
import InsightsRoundedIcon from '@mui/icons-material/InsightsRounded';
import ListAltRoundedIcon from '@mui/icons-material/ListAltRounded';
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded';
import RemoveCircleOutlineRoundedIcon from '@mui/icons-material/RemoveCircleOutlineRounded';
import ShoppingCartRoundedIcon from '@mui/icons-material/ShoppingCartRounded';
import SummarizeRoundedIcon from '@mui/icons-material/SummarizeRounded';
import { ApiError, api } from './api';

const INITIAL_EVENT_FORM = {
  name: '',
  description: '',
  venue: '',
  eventDate: '',
  startTime: '19:30',
  totalTicketCapacity: 300,
  pricingTiers: [createEmptyTier()]
};

const MENU_SECTIONS = [
  {
    key: 'events',
    label: 'Events',
    icon: <EventRoundedIcon fontSize="small" />,
    items: [{ key: 'events.manage', label: 'Manage events', icon: <ListAltRoundedIcon fontSize="small" /> }]
  },
  {
    key: 'tickets',
    label: 'Tickets',
    icon: <ConfirmationNumberRoundedIcon fontSize="small" />,
    items: [
      { key: 'tickets.availability', label: 'Availability', icon: <ListAltRoundedIcon fontSize="small" /> },
      { key: 'tickets.purchase', label: 'Purchase tickets', icon: <ShoppingCartRoundedIcon fontSize="small" /> }
    ]
  },
  {
    key: 'reports',
    label: 'Reports',
    icon: <InsightsRoundedIcon fontSize="small" />,
    items: [
      { key: 'reports.all', label: 'All sales summary', icon: <SummarizeRoundedIcon fontSize="small" /> },
      { key: 'reports.single', label: 'Single event summary', icon: <SummarizeRoundedIcon fontSize="small" /> }
    ]
  }
];

export default function App() {
  const [selectedView, setSelectedView] = useState('events.manage');
  const [expandedSections, setExpandedSections] = useState(() =>
    Object.fromEntries(MENU_SECTIONS.map((section) => [section.key, true]))
  );
  const [snackbar, setSnackbar] = useState({ open: false, severity: 'success', message: '' });

  const notify = (severity, message) => {
    setSnackbar({ open: true, severity, message });
  };

  const content = useMemo(() => {
    switch (selectedView) {
      case 'tickets.availability':
        return <TicketsView mode="availability" notify={notify} />;
      case 'tickets.purchase':
        return <TicketsView mode="purchase" notify={notify} />;
      case 'reports.all':
        return <ReportsView mode="all" notify={notify} />;
      case 'reports.single':
        return <ReportsView mode="single" notify={notify} />;
      case 'events.manage':
      default:
        return <EventsView notify={notify} />;
    }
  }, [selectedView]);

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="static" color="transparent" elevation={0} sx={{ borderBottom: '1px solid #e6ebf2' }}>
        <Toolbar sx={{ gap: 2 }}>
          <EventRoundedIcon color="primary" />
          <Box>
            <Typography variant="h6">Ticketing Dashboard</Typography>
            <Typography variant="body2" color="text.secondary">
             Events, tickets, and reporting system
            </Typography>
          </Box>
        </Toolbar>
      </AppBar>

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', lg: '280px minmax(0, 1fr)' },
          gap: 3,
          p: 3,
          alignItems: 'start'
        }}
      >
        <Paper sx={{ p: 2, position: { lg: 'sticky' }, top: { lg: 24 } }}>
          <Typography variant="subtitle1" sx={{ mb: 1.5 }}>
            Navigation
          </Typography>
          <List disablePadding>
            {MENU_SECTIONS.map((section) => {
              const expanded = expandedSections[section.key];
              return (
                <Box key={section.key}>
                  <ListItemButton onClick={() => setExpandedSections((current) => ({ ...current, [section.key]: !current[section.key] }))}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexGrow: 1 }}>
                      {section.icon}
                      <ListItemText primary={section.label} primaryTypographyProps={{ fontWeight: 600 }} />
                    </Box>
                    {expanded ? <ExpandLessRoundedIcon fontSize="small" /> : <ExpandMoreRoundedIcon fontSize="small" />}
                  </ListItemButton>
                  <Collapse in={expanded} timeout="auto" unmountOnExit>
                    <List disablePadding>
                      {section.items.map((item) => (
                        <ListItemButton
                          key={item.key}
                          sx={{ pl: 4, borderRadius: 2, ml: 1 }}
                          selected={selectedView === item.key}
                          onClick={() => setSelectedView(item.key)}
                        >
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25 }}>
                            {item.icon}
                            <ListItemText primary={item.label} />
                          </Box>
                        </ListItemButton>
                      ))}
                    </List>
                  </Collapse>
                </Box>
              );
            })}
          </List>
        </Paper>

        <Box sx={{ minWidth: 0 }}>{content}</Box>
      </Box>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4500}
        onClose={() => setSnackbar((current) => ({ ...current, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          severity={snackbar.severity}
          variant="filled"
          onClose={() => setSnackbar((current) => ({ ...current, open: false }))}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}

function EventsView({ notify }) {
  const [form, setForm] = useState(INITIAL_EVENT_FORM);
  const [editingEventId, setEditingEventId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [page, setPage] = useState(1);
  const [eventPage, setEventPage] = useState({
    items: [],
    pageNumber: 1,
    pageSize: 20,
    totalItems: 0,
    totalPages: 1
  });

  useEffect(() => {
    loadEvents(page);
  }, [page]);

  async function loadEvents(targetPage = page) {
    setLoading(true);
    try {
      const response = await api.getEvents(targetPage, 20);
      setEventPage(response);
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setLoading(false);
    }
  }

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function updateTier(index, field, value) {
    setForm((current) => ({
      ...current,
      pricingTiers: current.pricingTiers.map((tier, tierIndex) =>
        tierIndex === index ? { ...tier, [field]: value } : tier
      )
    }));
  }

  function addTier() {
    setForm((current) => ({ ...current, pricingTiers: [...current.pricingTiers, createEmptyTier()] }));
  }

  function removeTier(index) {
    setForm((current) => ({
      ...current,
      pricingTiers:
        current.pricingTiers.length === 1
          ? [createEmptyTier()]
          : current.pricingTiers.filter((_, tierIndex) => tierIndex !== index)
    }));
  }

  function resetForm() {
    setEditingEventId(null);
    setForm(INITIAL_EVENT_FORM);
  }

  function startEditing(eventItem) {
    setEditingEventId(eventItem.id);
    setForm({
      name: eventItem.name ?? '',
      description: eventItem.description ?? '',
      venue: eventItem.venue ?? '',
      eventDate: eventItem.eventDate ?? '',
      startTime: normalizeTimeForInput(eventItem.startTime),
      totalTicketCapacity: eventItem.totalTicketCapacity ?? 0,
      pricingTiers:
        eventItem.pricingTiers?.map((tier) => ({
          name: tier.name ?? '',
          price: tier.price ?? 0,
          capacity: tier.capacity ?? 0
        })) ?? [createEmptyTier()]
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setSaving(true);

    try {
      const payload = serializeEventForm(form);
      if (editingEventId) {
        await api.updateEvent(editingEventId, payload);
        notify('success', 'Event updated successfully.');
      } else {
        await api.createEvent(payload);
        notify('success', 'Event created successfully.');
      }

      resetForm();
      await loadEvents(page);
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(eventId) {
    if (!window.confirm('Delete this event? This only works when no tickets have been sold.')) {
      return;
    }

    try {
      await api.deleteEvent(eventId);
      notify('success', 'Event deleted successfully.');

      const shouldMoveBack = eventPage.items.length === 1 && page > 1;
      const nextPage = shouldMoveBack ? page - 1 : page;
      if (nextPage !== page) {
        setPage(nextPage);
      } else {
        await loadEvents(nextPage);
      }

      if (editingEventId === eventId) {
        resetForm();
      }
    } catch (error) {
      notify('error', getReadableError(error));
    }
  }

  return (
    <Stack spacing={3}>
      <SectionHeader
        title="Events"
        description="Create, update, browse, and delete events with pricing tiers and pagination."
        action={
          <Button startIcon={<RefreshRoundedIcon />} variant="outlined" onClick={() => loadEvents(page)}>
            Refresh
          </Button>
        }
      />

      <Paper component="form" onSubmit={handleSubmit} sx={{ p: 3 }}>
        <Stack spacing={2.5}>
          <Box>
            <Typography variant="h6">{editingEventId ? 'Edit event' : 'Create event'}</Typography>
            <Typography variant="body2" color="text.secondary">
              {editingEventId
                ? 'Editing uses the PUT endpoint and replaces the pricing tiers.'
                : 'Use the POST endpoint to add a new event with one or more tiers.'}
            </Typography>
          </Box>

          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' },
              gap: 2
            }}
          >
            <TextField label="Event name" value={form.name} onChange={(e) => updateField('name', e.target.value)} required fullWidth />
            <TextField label="Venue" value={form.venue} onChange={(e) => updateField('venue', e.target.value)} required fullWidth />
            <TextField
              label="Event date"
              type="date"
              value={form.eventDate}
              onChange={(e) => updateField('eventDate', e.target.value)}
              InputLabelProps={{ shrink: true }}
              required
              fullWidth
            />
            <TextField
              label="Start time"
              type="time"
              value={form.startTime}
              onChange={(e) => updateField('startTime', e.target.value)}
              InputLabelProps={{ shrink: true }}
              inputProps={{ step: 60 }}
              required
              fullWidth
            />
            <TextField
              label="Total capacity"
              type="number"
              value={form.totalTicketCapacity}
              onChange={(e) => updateField('totalTicketCapacity', Number(e.target.value))}
              required
              fullWidth
            />
            <TextField
              label="Description"
              value={form.description}
              onChange={(e) => updateField('description', e.target.value)}
              multiline
              minRows={3}
              fullWidth
              sx={{ gridColumn: { md: '1 / -1' } }}
            />
          </Box>

          <Divider />

          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Box>
              <Typography variant="subtitle1">Pricing tiers</Typography>
              <Typography variant="body2" color="text.secondary">
                Add simple tiers like Standard, Premium, and VIP.
              </Typography>
            </Box>
            <Button startIcon={<AddCircleOutlineRoundedIcon />} onClick={addTier}>
              Add tier
            </Button>
          </Stack>

          <Stack spacing={2}>
            {form.pricingTiers.map((tier, index) => (
              <Paper key={`${index}-${tier.name}`} sx={{ p: 2, bgcolor: '#fbfcff' }}>
                <Box
                  sx={{
                    display: 'grid',
                    gridTemplateColumns: { xs: '1fr', md: '2fr 1fr 1fr auto' },
                    gap: 2,
                    alignItems: 'center'
                  }}
                >
                  <TextField
                    label="Tier name"
                    value={tier.name}
                    onChange={(e) => updateTier(index, 'name', e.target.value)}
                    required
                    fullWidth
                  />
                  <TextField
                    label="Price"
                    type="number"
                    value={tier.price}
                    onChange={(e) => updateTier(index, 'price', Number(e.target.value))}
                    required
                    fullWidth
                  />
                  <TextField
                    label="Capacity"
                    type="number"
                    value={tier.capacity}
                    onChange={(e) => updateTier(index, 'capacity', Number(e.target.value))}
                    required
                    fullWidth
                  />
                  <IconButton color="error" onClick={() => removeTier(index)} aria-label="Remove tier">
                    <RemoveCircleOutlineRoundedIcon />
                  </IconButton>
                </Box>
              </Paper>
            ))}
          </Stack>

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
            <Button type="submit" variant="contained" disabled={saving}>
              {saving ? 'Saving...' : editingEventId ? 'Update event' : 'Create event'}
            </Button>
            <Button type="button" variant="outlined" onClick={resetForm}>
              Clear form
            </Button>
          </Stack>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', md: 'center' }} spacing={1.5} sx={{ mb: 2 }}>
          <Box>
            <Typography variant="h6">Event list</Typography>
          </Box>
          <Chip label={`Total events: ${eventPage.totalItems ?? 0}`} color="primary" variant="outlined" />
        </Stack>

        {loading ? (
          <CenteredLoader />
        ) : (
          <>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Venue</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell>Time</TableCell>
                    <TableCell align="right">Capacity</TableCell>
                    <TableCell>Pricing tiers</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {eventPage.items?.length ? (
                    eventPage.items.map((eventItem) => (
                      <TableRow key={eventItem.id} hover>
                        <TableCell>
                          <Stack spacing={0.5}>
                            <Typography fontWeight={600}>{eventItem.name}</Typography>
                            <Typography variant="caption" color="text.secondary">
                              {eventItem.description}
                            </Typography>
                          </Stack>
                        </TableCell>
                        <TableCell>{eventItem.venue}</TableCell>
                        <TableCell>{eventItem.eventDate}</TableCell>
                        <TableCell>{normalizeTimeForInput(eventItem.startTime)}</TableCell>
                        <TableCell align="right">{eventItem.totalTicketCapacity}</TableCell>
                        <TableCell>
                          <Stack direction="row" gap={1} flexWrap="wrap">
                            {eventItem.pricingTiers?.map((tier) => (
                              <Chip
                                key={tier.id}
                                size="small"
                                label={`${tier.name}: ${formatCurrency(tier.price)} / ${tier.capacity}`}
                              />
                            ))}
                          </Stack>
                        </TableCell>
                        <TableCell align="right">
                          <Stack direction="row" justifyContent="flex-end" spacing={1}>
                            <Button size="small" startIcon={<EditRoundedIcon />} onClick={() => startEditing(eventItem)}>
                              Edit
                            </Button>
                            <Button
                              size="small"
                              color="error"
                              startIcon={<DeleteOutlineRoundedIcon />}
                              onClick={() => handleDelete(eventItem.id)}
                            >
                              Delete
                            </Button>
                          </Stack>
                        </TableCell>
                      </TableRow>
                    ))
                  ) : (
                    <TableRow>
                      <TableCell colSpan={7} align="center">
                        No events found.
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>

            {(eventPage.totalPages ?? 1) > 1 && (
              <Stack direction="row" justifyContent="center" sx={{ mt: 2 }}>
                <Pagination count={eventPage.totalPages} page={page} onChange={(_, value) => setPage(value)} color="primary" />
              </Stack>
            )}
          </>
        )}
      </Paper>
    </Stack>
  );
}

function TicketsView({ mode, notify }) {
  const [events, setEvents] = useState([]);
  const [loadingEvents, setLoadingEvents] = useState(true);
  const [loadingAvailability, setLoadingAvailability] = useState(false);
  const [purchasing, setPurchasing] = useState(false);
  const [availability, setAvailability] = useState(null);
  const [purchaseResult, setPurchaseResult] = useState(null);
  const [selectedEventId, setSelectedEventId] = useState('');
  const [purchaseForm, setPurchaseForm] = useState({
    eventId: '',
    pricingTierId: '',
    customerName: '',
    customerEmail: '',
    quantity: 1
  });

  useEffect(() => {
    loadEvents();
  }, []);

  useEffect(() => {
    setPurchaseForm((current) => ({ ...current, eventId: selectedEventId }));
    setAvailability(null);
    setPurchaseResult(null);
  }, [selectedEventId]);

  async function loadEvents() {
    setLoadingEvents(true);
    try {
      const response = await api.getEvents(1, 100);
      setEvents(response.items ?? []);
      if ((response.items?.length ?? 0) > 0) {
        setSelectedEventId((current) => current || response.items[0].id);
      }
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setLoadingEvents(false);
    }
  }

  async function loadAvailability(eventId = selectedEventId) {
    if (!eventId) {
      notify('warning', 'Select an event first.');
      return;
    }

    setLoadingAvailability(true);
    try {
      const response = await api.getAvailability(eventId);
      setAvailability(response);
      const firstTierId = response.availabilityByTier?.[0]?.pricingTierId ?? '';
      setPurchaseForm((current) => ({
        ...current,
        eventId,
        pricingTierId: response.availabilityByTier?.some((tier) => tier.pricingTierId === current.pricingTierId)
          ? current.pricingTierId
          : firstTierId
      }));
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setLoadingAvailability(false);
    }
  }

  async function handlePurchase(event) {
    event.preventDefault();
    setPurchasing(true);

    try {
      const response = await api.purchaseTickets({
        ...purchaseForm,
        quantity: Number(purchaseForm.quantity)
      });
      setPurchaseResult(response);
      notify('success', `Purchase created. Booking reference: ${response.bookingReference}`);
      await loadAvailability(purchaseForm.eventId);
      setPurchaseForm((current) => ({ ...current, customerName: '', customerEmail: '', quantity: 1 }));
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setPurchasing(false);
    }
  }

  return (
    <Stack spacing={3}>
      <SectionHeader
        title="Tickets"
        description="Check availability by event and purchase tickets for a specific pricing tier."
        action={
          <Button startIcon={<RefreshRoundedIcon />} variant="outlined" onClick={loadEvents}>
            Refresh events
          </Button>
        }
      />

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <Typography variant="h6">Availability lookup</Typography>
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: 'minmax(0, 1fr) auto' },
              gap: 2,
              alignItems: 'start'
            }}
          >
            <TextField
              select
              label="Event"
              value={selectedEventId}
              onChange={(e) => setSelectedEventId(e.target.value)}
              disabled={loadingEvents}
              fullWidth
            >
              {events.map((eventItem) => (
                <MenuItem key={eventItem.id} value={eventItem.id}>
                  {eventItem.name} — {eventItem.eventDate}
                </MenuItem>
              ))}
            </TextField>
            <Button variant="contained" onClick={() => loadAvailability()} disabled={loadingEvents || loadingAvailability} sx={{ minWidth: 180, height: 56 }}>
              {loadingAvailability ? 'Loading...' : 'Load availability'}
            </Button>
          </Box>
        </Stack>
      </Paper>

      {loadingAvailability ? (
        <Paper sx={{ p: 3 }}>
          <CenteredLoader />
        </Paper>
      ) : availability ? (
        <Paper sx={{ p: 3 }}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={1.5} justifyContent="space-between" sx={{ mb: 2 }}>
            <Box>
              <Typography variant="h6">Event availability</Typography>
              <Typography variant="body2" color="text.secondary">
                Event ID: {availability.eventId}
              </Typography>
            </Box>
            <Stack direction="row" spacing={1} flexWrap="wrap">
              <Chip label={`Capacity: ${availability.totalCapacity}`} />
              <Chip label={`Sold: ${availability.totalTicketsSold}`} color="secondary" variant="outlined" />
              <Chip label={`Available: ${availability.totalTicketsAvailable}`} color="success" variant="outlined" />
            </Stack>
          </Stack>

          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Tier</TableCell>
                  <TableCell align="right">Price</TableCell>
                  <TableCell align="right">Capacity</TableCell>
                  <TableCell align="right">Sold</TableCell>
                  <TableCell align="right">Available</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {availability.availabilityByTier?.map((tier) => {
                  const eventItem = events.find((item) => item.id === availability.eventId);
                  const matchingTier = eventItem?.pricingTiers?.find((candidate) => candidate.id === tier.pricingTierId);
                  return (
                    <TableRow key={tier.pricingTierId}>
                      <TableCell>{tier.name}</TableCell>
                      <TableCell align="right">{matchingTier ? formatCurrency(matchingTier.price) : '—'}</TableCell>
                      <TableCell align="right">{tier.capacity}</TableCell>
                      <TableCell align="right">{tier.ticketsSold}</TableCell>
                      <TableCell align="right">{tier.ticketsAvailable}</TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      ) : null}

      {mode === 'purchase' && (
        <Paper component="form" onSubmit={handlePurchase} sx={{ p: 3 }}>
          <Stack spacing={2.5}>
            <Box>
              <Typography variant="h6">Purchase tickets</Typography>
              <Typography variant="body2" color="text.secondary">
                Choose an event and tier, then submit the ticket purchase request.
              </Typography>
            </Box>

            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' },
                gap: 2
              }}
            >
              <TextField
                select
                label="Event"
                value={purchaseForm.eventId}
                onChange={(e) => {
                  setSelectedEventId(e.target.value);
                  setPurchaseForm((current) => ({ ...current, eventId: e.target.value, pricingTierId: '' }));
                }}
                required
                fullWidth
              >
                {events.map((eventItem) => (
                  <MenuItem key={eventItem.id} value={eventItem.id}>
                    {eventItem.name}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                label="Pricing tier"
                value={purchaseForm.pricingTierId}
                onChange={(e) => setPurchaseForm((current) => ({ ...current, pricingTierId: e.target.value }))}
                required
                fullWidth
                helperText={!availability ? 'Load availability to populate tiers.' : ''}
              >
                {(availability?.availabilityByTier ?? []).map((tier) => (
                  <MenuItem key={tier.pricingTierId} value={tier.pricingTierId}>
                    {tier.name} — {tier.ticketsAvailable} available
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                label="Customer name"
                value={purchaseForm.customerName}
                onChange={(e) => setPurchaseForm((current) => ({ ...current, customerName: e.target.value }))}
                required
                fullWidth
              />
              <TextField
                label="Customer email"
                type="email"
                value={purchaseForm.customerEmail}
                onChange={(e) => setPurchaseForm((current) => ({ ...current, customerEmail: e.target.value }))}
                required
                fullWidth
              />
              <TextField
                label="Quantity"
                type="number"
                value={purchaseForm.quantity}
                onChange={(e) => setPurchaseForm((current) => ({ ...current, quantity: Number(e.target.value) }))}
                inputProps={{ min: 1 }}
                required
                fullWidth
              />
            </Box>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
              <Button type="submit" variant="contained" disabled={purchasing || !purchaseForm.pricingTierId}>
                {purchasing ? 'Purchasing...' : 'Purchase tickets'}
              </Button>
              <Button type="button" variant="outlined" onClick={() => loadAvailability(purchaseForm.eventId)}>
                Reload availability
              </Button>
            </Stack>
          </Stack>
        </Paper>
      )}

      {purchaseResult && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>
            Latest purchase result
          </Typography>
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' },
              gap: 1.5
            }}
          >
            <DetailRow label="Purchase ID" value={purchaseResult.id} />
            <DetailRow label="Booking reference" value={purchaseResult.bookingReference} />
            <DetailRow label="Customer" value={purchaseResult.customerName} />
            <DetailRow label="Email" value={purchaseResult.customerEmail} />
            <DetailRow label="Quantity" value={purchaseResult.quantity} />
            <DetailRow label="Unit price" value={formatCurrency(purchaseResult.unitPrice)} />
            <DetailRow label="Total price" value={formatCurrency(purchaseResult.totalPrice)} />
            <DetailRow label="Status" value={purchaseResult.status} />
            <DetailRow label="Purchased" value={formatDateTime(purchaseResult.purchaseDateUtc)} />
          </Box>
        </Paper>
      )}
    </Stack>
  );
}

function ReportsView({ mode, notify }) {
  const [events, setEvents] = useState([]);
  const [loadingEvents, setLoadingEvents] = useState(true);
  const [loadingAll, setLoadingAll] = useState(mode === 'all');
  const [loadingSingle, setLoadingSingle] = useState(false);
  const [allSummaries, setAllSummaries] = useState([]);
  const [selectedEventId, setSelectedEventId] = useState('');
  const [singleSummary, setSingleSummary] = useState(null);

  useEffect(() => {
    loadEvents();
    if (mode === 'all') {
      loadAllSummaries();
    }
  }, [mode]);

  async function loadEvents() {
    setLoadingEvents(true);
    try {
      const response = await api.getEvents(1, 100);
      setEvents(response.items ?? []);
      if ((response.items?.length ?? 0) > 0) {
        setSelectedEventId((current) => current || response.items[0].id);
      }
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setLoadingEvents(false);
    }
  }

  async function loadAllSummaries() {
    setLoadingAll(true);
    try {
      const response = await api.getAllSalesSummaries();
      setAllSummaries(response ?? []);
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setLoadingAll(false);
    }
  }

  async function loadSingleSummary() {
    if (!selectedEventId) {
      notify('warning', 'Select an event first.');
      return;
    }

    setLoadingSingle(true);
    try {
      const response = await api.getEventSalesSummary(selectedEventId);
      setSingleSummary(response);
    } catch (error) {
      notify('error', getReadableError(error));
    } finally {
      setLoadingSingle(false);
    }
  }

  return (
    <Stack spacing={3}>
      <SectionHeader
        title="Reports"
        description="Review revenue and ticket sales across all events or inspect one event in detail."
        action={
          <Button
            startIcon={<RefreshRoundedIcon />}
            variant="outlined"
            onClick={mode === 'all' ? loadAllSummaries : loadSingleSummary}
          >
            Refresh report
          </Button>
        }
      />

      {mode === 'single' && (
        <Paper sx={{ p: 3 }}>
          <Stack spacing={2}>
            <Typography variant="h6">Single event summary</Typography>
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', md: 'minmax(0, 1fr) auto' },
                gap: 2
              }}
            >
              <TextField
                select
                label="Event"
                value={selectedEventId}
                onChange={(e) => setSelectedEventId(e.target.value)}
                disabled={loadingEvents}
                fullWidth
              >
                {events.map((eventItem) => (
                  <MenuItem key={eventItem.id} value={eventItem.id}>
                    {eventItem.name}
                  </MenuItem>
                ))}
              </TextField>
              <Button variant="contained" onClick={loadSingleSummary} disabled={loadingSingle || loadingEvents} sx={{ minWidth: 180, height: 56 }}>
                {loadingSingle ? 'Loading...' : 'Load event summary'}
              </Button>
            </Box>
          </Stack>
        </Paper>
      )}

      {mode === 'single' && singleSummary && <SummaryCard title="Selected event summary" summary={singleSummary} />}

      {mode === 'all' && (
        <Paper sx={{ p: 3 }}>
          <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', md: 'center' }} spacing={1.5} sx={{ mb: 2 }}>
            <Box>
              <Typography variant="h6">All sales summaries</Typography>
              <Typography variant="body2" color="text.secondary">
                GET `/api/v1/reports/events/sales-summary`
              </Typography>
            </Box>
            <Button variant="outlined" onClick={loadAllSummaries} startIcon={<RefreshRoundedIcon />}>
              Reload all summaries
            </Button>
          </Stack>

          {loadingAll ? (
            <CenteredLoader />
          ) : allSummaries.length ? (
            <Stack spacing={2}>
              {allSummaries.map((summary) => (
                <SummaryCard key={summary.eventId} title={summary.eventName} summary={summary} compact />
              ))}
            </Stack>
          ) : (
            <Typography color="text.secondary">No sales summaries available yet.</Typography>
          )}
        </Paper>
      )}
    </Stack>
  );
}

function SummaryCard({ title, summary, compact = false }) {
  return (
    <Paper sx={{ p: 3, bgcolor: compact ? '#fbfcff' : '#fff' }}>
      <Stack spacing={2}>
        <Box>
          <Typography variant="h6">{title}</Typography>
          <Typography variant="body2" color="text.secondary">
            Event ID: {summary.eventId}
          </Typography>
        </Box>

        <Stack direction="row" flexWrap="wrap" gap={1}>
          <Chip label={`Capacity: ${summary.totalCapacity}`} />
          <Chip label={`Sold: ${summary.totalTicketsSold}`} color="secondary" variant="outlined" />
          <Chip label={`Remaining: ${summary.remainingTickets}`} color="success" variant="outlined" />
          <Chip label={`Revenue: ${formatCurrency(summary.totalRevenue)}`} color="primary" variant="outlined" />
        </Stack>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Tier</TableCell>
                <TableCell align="right">Tickets sold</TableCell>
                <TableCell align="right">Revenue</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {summary.salesByTier?.map((tier) => (
                <TableRow key={tier.pricingTierId}>
                  <TableCell>{tier.pricingTierName}</TableCell>
                  <TableCell align="right">{tier.ticketsSold}</TableCell>
                  <TableCell align="right">{formatCurrency(tier.revenue)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>
    </Paper>
  );
}

function SectionHeader({ title, description, action }) {
  return (
    <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" spacing={2} alignItems={{ xs: 'flex-start', md: 'center' }}>
      <Box>
        <Typography variant="h4" gutterBottom>
          {title}
        </Typography>
        <Typography color="text.secondary">{description}</Typography>
      </Box>
      {action}
    </Stack>
  );
}

function CenteredLoader() {
  return (
    <Stack alignItems="center" justifyContent="center" sx={{ py: 5 }}>
      <CircularProgress />
    </Stack>
  );
}

function DetailRow({ label, value }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography>{value || '—'}</Typography>
    </Box>
  );
}

function createEmptyTier() {
  return {
    name: '',
    price: 0,
    capacity: 0
  };
}

function serializeEventForm(form) {
  return {
    name: form.name.trim(),
    description: form.description.trim(),
    venue: form.venue.trim(),
    eventDate: form.eventDate,
    startTime: normalizeTimeForApi(form.startTime),
    totalTicketCapacity: Number(form.totalTicketCapacity),
    pricingTiers: form.pricingTiers.map((tier) => ({
      name: tier.name.trim(),
      price: Number(tier.price),
      capacity: Number(tier.capacity)
    }))
  };
}

function normalizeTimeForInput(value) {
  if (!value) {
    return '';
  }

  return value.length >= 5 ? value.slice(0, 5) : value;
}

function normalizeTimeForApi(value) {
  if (!value) {
    return '';
  }

  return value.length === 5 ? `${value}:00` : value;
}

function formatCurrency(value) {
  const amount = Number(value ?? 0);
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(amount);
}

function formatDateTime(value) {
  if (!value) {
    return '—';
  }

  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(new Date(value));
}

function getReadableError(error) {
  if (error instanceof ApiError) {
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return 'Something went wrong.';
}



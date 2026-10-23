import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('../../../app/api/agent', () => ({
  default: {
    Activities: { details: vi.fn(), update: vi.fn(), create: vi.fn(), list: vi.fn() },
    Account: {},
    Profiles: {},
  },
}));
vi.mock('../../../app/routes', () => ({ router: { navigate: vi.fn() } }));

import agent from '../../../app/api/agent';
import type { Activity } from '../../../app/models/activity';
import { store } from '../../../app/stores/store';
import ActivityForm from './ActivityForm';

const activity = {
  id: '1',
  title: 'Old title',
  date: new Date(2026, 10, 5, 18, 0),
  description: 'Some description',
  category: 'music',
  city: 'Rostov',
  venue: 'Club',
  hostUsername: 'bob',
  isCancelled: false,
  isGoing: false,
  isHost: true,
  attendees: [],
} as Activity;

function renderForm(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/createActivity" element={<ActivityForm />} />
        <Route path="/manage/:id" element={<ActivityForm />} />
        <Route path="/activities/:id" element={<p>Details page</p>} />
      </Routes>
    </MemoryRouter>
  );
}

describe('ActivityForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    store.activityStore.activityRegistry.clear();
    store.activityStore.selectedActivity = undefined;
    store.activityStore.loadingInitial = false;
  });

  afterEach(() => cleanup());

  it('keeps Submit disabled and shows a required error for an empty title', async () => {
    renderForm('/createActivity');

    expect(screen.getByRole('button', { name: 'Submit' })).toBeDisabled();

    await userEvent.click(screen.getByPlaceholderText('Title'));
    await userEvent.tab();

    expect(await screen.findByText('The activity title is required')).toBeInTheDocument();
  });

  it('loads an event, saves changes and goes to the event page', async () => {
    vi.mocked(agent.Activities.details).mockResolvedValue(activity);
    vi.mocked(agent.Activities.update).mockResolvedValue(undefined);

    renderForm('/manage/1');

    await waitFor(() => expect(screen.getByPlaceholderText('Title')).toHaveValue('Old title'));
    const title = screen.getByPlaceholderText('Title');
    const submit = screen.getByRole('button', { name: 'Submit' });
    expect(submit).toBeDisabled();

    await userEvent.clear(title);
    await userEvent.type(title, 'New title');
    await waitFor(() => expect(submit).toBeEnabled());
    await userEvent.click(submit);

    expect(await screen.findByText('Details page')).toBeInTheDocument();
    expect(agent.Activities.update).toHaveBeenCalledWith(
      expect.objectContaining({ id: '1', title: 'New title' })
    );
  });
});
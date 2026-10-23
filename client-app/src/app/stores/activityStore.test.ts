import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('../api/agent', () => ({
  default: {
    Activities: { list: vi.fn() },
    Account: {},
    Profiles: {},
  },
}));
vi.mock('../routes', () => ({ router: { navigate: vi.fn() } }));

import './store';
import agent from '../api/agent';
import type { Activity } from '../models/activity';
import { PaginatedResult, PagingParams } from '../models/paginations';
import ActivityStore from './activityStore';

const list = vi.mocked(agent.Activities.list);

const pagination = { currentPage: 1, itemsPerPage: 10, totalItems: 3, totalPages: 3 };

const makeActivity = (id: string, date: Date) =>
  ({
    id,
    title: id,
    date,
    description: '',
    category: 'music',
    city: 'city',
    venue: 'venue',
    hostUsername: 'host',
    isCancelled: false,
    isGoing: false,
    isHost: false,
    attendees: [],
  }) as Activity;

const result = (...activities: Activity[]) => new PaginatedResult(activities, pagination);

describe('ActivityStore', () => {
  let activityStore: ActivityStore;

  beforeEach(() => {
    vi.clearAllMocks();
    list.mockResolvedValue(new PaginatedResult([], pagination));
    activityStore = new ActivityStore();
  });

  it('loads activities, sorts them and groups by day', async () => {
    const a = makeActivity('a', new Date(2026, 10, 2, 10, 0));
    const b = makeActivity('b', new Date(2026, 10, 2, 15, 0));
    const c = makeActivity('c', new Date(2026, 10, 3, 9, 0));
    list.mockResolvedValueOnce(result(c, b, a));

    await activityStore.loadActivities();

    expect(
      activityStore.groupedActivities.map(([day, items]) => [day, items.map((x) => x.id)])
    ).toEqual([
      ['02 Nov 2026', ['a', 'b']],
      ['03 Nov 2026', ['c']],
    ]);
    expect(activityStore.pagination?.totalPages).toBe(3);
    expect(activityStore.loadingInitial).toBe(false);
  });

  it('sends paging and the default filter to the API', async () => {
    await activityStore.loadActivities();

    const params = list.mock.calls[0][0];
    expect(params.get('pageNumber')).toBe('1');
    expect(params.get('all')).toBe('true');
    expect(params.has('startDate')).toBe(false);
  });

  it('reloads from the first page when the filter changes', async () => {
    list.mockResolvedValueOnce(result(makeActivity('a', new Date(2026, 10, 2))));
    await activityStore.loadActivities();
    activityStore.setPagingParams(new PagingParams(3));
    list.mockClear();

    activityStore.setFilter('isGoing');

    await vi.waitFor(() => expect(activityStore.loadingInitial).toBe(false));
    expect(list).toHaveBeenCalledTimes(1);
    const params = list.mock.calls[0][0];
    expect(params.get('isGoing')).toBe('true');
    expect(params.has('all')).toBe(false);
    expect(params.get('pageNumber')).toBe('1');
    expect(activityStore.activityRegistry.size).toBe(0);
  });

  it('does not reload when the same filter is selected again', () => {
    activityStore.setFilter('all');

    expect(list).not.toHaveBeenCalled();
  });

  it('sends the selected start date', async () => {
    const date = new Date(2026, 10, 5);

    activityStore.setStartDate(date);

    await vi.waitFor(() => expect(list).toHaveBeenCalled());
    expect(list.mock.calls[0][0].get('startDate')).toBe(date.toISOString());
  });
});
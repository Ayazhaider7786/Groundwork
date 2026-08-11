import dayjs from 'dayjs';
import duration from 'dayjs/plugin/duration';
import relativeTime from 'dayjs/plugin/relativeTime';
import utc from 'dayjs/plugin/utc';

dayjs.extend(utc);
dayjs.extend(relativeTime);
dayjs.extend(duration);

// The API stores and returns UTC; every helper converts to local for display.
const toLocal = (value: string) => dayjs.utc(value).local();

export const formatDateShort = (value: string): string => toLocal(value).format('D MMM YYYY');

export const formatDateTime = (value: string): string => toLocal(value).format('D MMM YYYY, HH:mm');

export const formatRelative = (value: string): string => toLocal(value).fromNow();

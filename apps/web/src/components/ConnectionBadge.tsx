import type { ConnectionStatus } from '../todos/useTodoHub'

const LABELS: Record<ConnectionStatus, string> = {
  connecting: 'Connecting',
  connected: 'Live',
  reconnecting: 'Reconnecting',
  disconnected: 'Offline',
}

export function ConnectionBadge({ status }: { status: ConnectionStatus }) {
  return (
    <span className={`badge badge-${status}`} role="status">
      <span className="badge-dot" />
      {LABELS[status]}
    </span>
  )
}

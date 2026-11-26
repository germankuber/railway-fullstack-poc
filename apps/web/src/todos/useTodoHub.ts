import { useEffect, useState } from 'react'
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { API_URL } from '../config'
import { tokenStorage } from '../api/tokenStorage'
import type { Todo } from '../api/types'

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

interface TodoHubHandlers {
  onCreated: (todo: Todo) => void
  onUpdated: (todo: Todo) => void
  onDeleted: (id: string) => void
  onConnected: () => void
}

export function useTodoHub(handlers: TodoHubHandlers): ConnectionStatus {
  const [status, setStatus] = useState<ConnectionStatus>('connecting')
  const { onCreated, onUpdated, onDeleted, onConnected } = handlers

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/todos`, { accessTokenFactory: () => tokenStorage.get() ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('TodoCreated', onCreated)
    connection.on('TodoUpdated', onUpdated)
    connection.on('TodoDeleted', onDeleted)
    connection.onreconnecting(() => setStatus('reconnecting'))
    connection.onreconnected(() => {
      setStatus('connected')
      onConnected()
    })
    connection.onclose(() => setStatus('disconnected'))

    let disposed = false

    connection
      .start()
      .then(() => {
        if (!disposed && connection.state === HubConnectionState.Connected) {
          setStatus('connected')
          onConnected()
        }
      })
      .catch(() => {
        if (!disposed) {
          setStatus('disconnected')
        }
      })

    return () => {
      disposed = true
      void connection.stop()
    }
  }, [onCreated, onUpdated, onDeleted, onConnected])

  return status
}

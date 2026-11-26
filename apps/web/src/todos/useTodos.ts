import { useCallback, useEffect, useReducer, useState } from 'react'
import { todosApi } from '../api/client'
import type { Todo } from '../api/types'
import { todoReducer } from './todoReducer'
import { useTodoHub } from './useTodoHub'
import type { ConnectionStatus } from './useTodoHub'

interface UseTodosResult {
  todos: Todo[]
  status: ConnectionStatus
  error: string | null
  add: (title: string) => Promise<void>
  toggle: (todo: Todo) => Promise<void>
  remove: (id: string) => Promise<void>
}

const messageOf = (error: unknown): string => (error instanceof Error ? error.message : 'Something went wrong')

export function useTodos(): UseTodosResult {
  const [todos, dispatch] = useReducer(todoReducer, [])
  const [error, setError] = useState<string | null>(null)

  const refresh = useCallback(async () => {
    try {
      dispatch({ type: 'loaded', todos: await todosApi.list() })
      setError(null)
    } catch (failure) {
      setError(messageOf(failure))
    }
  }, [])

  const onUpserted = useCallback((todo: Todo) => dispatch({ type: 'upserted', todo }), [])
  const onDeleted = useCallback((id: string) => dispatch({ type: 'removed', id }), [])

  const status = useTodoHub({
    onCreated: onUpserted,
    onUpdated: onUpserted,
    onDeleted,
    onConnected: refresh,
  })

  useEffect(() => {
    void refresh()
  }, [refresh])

  const run = useCallback(async (action: () => Promise<void>) => {
    try {
      await action()
      setError(null)
    } catch (failure) {
      setError(messageOf(failure))
    }
  }, [])

  const add = useCallback(
    (title: string) => run(async () => onUpserted(await todosApi.create(title))),
    [run, onUpserted],
  )

  const toggle = useCallback(
    (todo: Todo) => run(async () => onUpserted(await todosApi.setDone(todo.id, !todo.isDone))),
    [run, onUpserted],
  )

  const remove = useCallback(
    (id: string) =>
      run(async () => {
        await todosApi.remove(id)
        onDeleted(id)
      }),
    [run, onDeleted],
  )

  return { todos, status, error, add, toggle, remove }
}

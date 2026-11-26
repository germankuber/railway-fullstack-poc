import type { Todo } from '../api/types'

export type TodoAction =
  | { type: 'loaded'; todos: Todo[] }
  | { type: 'upserted'; todo: Todo }
  | { type: 'removed'; id: string }

export function todoReducer(state: Todo[], action: TodoAction): Todo[] {
  switch (action.type) {
    case 'loaded':
      return action.todos
    case 'upserted': {
      const exists = state.some((todo) => todo.id === action.todo.id)
      return exists
        ? state.map((todo) => (todo.id === action.todo.id ? action.todo : todo))
        : [...state, action.todo]
    }
    case 'removed':
      return state.filter((todo) => todo.id !== action.id)
  }
}

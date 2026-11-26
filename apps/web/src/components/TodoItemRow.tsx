import type { Todo } from '../api/types'

interface TodoItemRowProps {
  todo: Todo
  onToggle: (todo: Todo) => void
  onDelete: (id: string) => void
}

export function TodoItemRow({ todo, onToggle, onDelete }: TodoItemRowProps) {
  return (
    <li className={todo.isDone ? 'todo done' : 'todo'}>
      <label>
        <input type="checkbox" checked={todo.isDone} onChange={() => onToggle(todo)} />
        <span>{todo.title}</span>
      </label>
      <button type="button" className="icon" aria-label={`Delete ${todo.title}`} onClick={() => onDelete(todo.id)}>
        ×
      </button>
    </li>
  )
}

import { Component, OnInit } from '@angular/core';
import {TodoService} from '../todo.service';
import {Todo} from '../todo/Todo';

@Component({
  selector: 'app-todo',
  templateUrl: './todo.component.html',
  styleUrls: ['./todo.component.less'],
  providers: [TodoService]
})
export class TodoComponent implements OnInit {
  todoList: Todo[];
  isError: false;
  errorMsg: string;

  constructor(private todoService: TodoService) {  }

  ngOnInit() {
    this.getTodos();
  }
  // Get todos
  getTodos() {
    this.todoService.getTodos().subscribe(
      todos => {
        this.todoList = todos;
      },
      err => {
        this.errorMsg = err.message;
      }
    );
  }
  // Add todo
  addTodo(taskDescription: string): void {
    taskDescription = taskDescription.trim();
    if (!taskDescription) { return; }
    this.todoService.addTodo({taskDescription} as Todo).subscribe(() => {this.getTodos(); });
  }
  // Delete todo
  deleteTodo(id: string): void {
    this.todoService.deleteTodo(id).subscribe(() => { this.getTodos(); });
  }
}

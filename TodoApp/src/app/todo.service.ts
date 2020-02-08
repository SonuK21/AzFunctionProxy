import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Todo } from './todo/Todo';

@Injectable({
  providedIn: 'root'
})

export class TodoService {
  baseApi = 'http://localhost:7071'; // 'https://az-fa-001.azurewebsites.net/api/todo'; //'http://localhost:7071';
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  constructor(private http: HttpClient) { }

  // Get todos
  getTodos(): Observable<Todo[]> {
    return this.http.get<Todo[]>(this.baseApi + '/api/todo').pipe(
      catchError(this.handleError<any>('Get todos!'))
    );
  }

  // Add todo
  addTodo(todo: Todo): Observable<Todo> {
    return this.http.post<Todo>(this.baseApi + '/api/todo', todo, this.httpOptions).pipe(
      catchError(this.handleError<any>('Add todo'))
    );
  }

  // Delete todo
  deleteTodo(id: string): Observable<any> {
    const apiEndpoint = this.baseApi + '/api/todo/' + id;
    return this.http.delete(apiEndpoint, this.httpOptions);
  }

  // Handle errors
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error); // log to console instead

      // tslint:disable-next-line: no-console
      console.info('${operation} failed: ${error.message}');

      // I am throwing an error to display an error on UI in case backend is down
      return throwError(error);

      // But you should Let the app keep running by returning an empty result.
      // return of(result as T);
    };
  }
}

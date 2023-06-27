# BookList
The web application allows users to track the progress of reading books, developed on the ASP.NET Core platform using the Minimal APIs. After registering a user, you can perform CRUD operations with books, set the reading status (to read, in progress, completed).

## Authentication and Authorization

To ensure the security of the application, authentication using JWT Bearer is used. Users must authenticate by providing a JWT (JSON Web Token) token to access the functionality of the application. Passwords are stored in a hashed form in the database.

## Database and ORM

The application uses a SQLite database to store information about users and their book lists. To work with the database, Object-Relational Mapping (ORM) is used using Entity Framework Core. Entity relationships are configured using FluentAPI.

## Data model

Each user in the application has a list of books containing the following information:

- Book title
- Name of the author of the book
- An array of genres the book belongs to
- Year of publication of the book
- Reading status: the status of the book, which can be one of three options: "To be read", "In progress" or "Read"

## API Endpoints

The application provides the following API Endpoints to perform operations:

### Auth
- `POST /register`: Register a new user.
- `POST /login`: Log in as a user, a token will be returned in response.

### User
- `GET /user`: Get the currently logged in user.
- `GET /users`: Get a list of all users.
- `GET /users/{id}`: Get user by id.
- `PUT/users/{id}`: Change user information by id.
- `DELETE/users/{id}`: Delete user by id.

### Book
- `GET /books`: Get a list of all books of the currently logged in user.
- `GET /users/{userId}/books`: Get a list of all user books by id.
- `GET /users/{userId}/books/{id}`: Get information about a specific book by its id and user id.
- `POST /users/{userId}/books`: Create a new book in the user list.
- `PUT /users/{userId}/books/{id}`: Update information about a specific book.
- `PUT /users/{userId}/books/{id}/status`: Update the read status of a book.
- `DELETE/users/{id}`: Delete a book from the user's list.

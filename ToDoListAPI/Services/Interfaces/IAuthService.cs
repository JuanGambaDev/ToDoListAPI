using ToDoListAPI.DTOs;
using ToDoListAPI.Models;
using ToDoListAPI.Repositories;

namespace ToDoListAPI.Services;

public interface IAuthService {

    Task<string> RegisterNewUser (UserRegisterRequest newUser);

}


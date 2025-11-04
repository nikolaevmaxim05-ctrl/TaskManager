using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;
using TaskManager.Infrastructure.DB.Repository;

namespace TaskManager.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IUserConfirmationRepository _userConfirmationRepository;
        public AuthorizationService(IUserRepository userRepo, ILogger<AuthorizationService> logger,
            IUserConfirmationRepository userConfirmationRepository)
        {
            _logger = logger;
            _userRepository = userRepo;
            _userConfirmationRepository = userConfirmationRepository;
        }
        public async Task<User?> LogIn (UserCreateDto userCreateDto)
        {
            _logger.LogInformation("Попытка войти в аккаунт");
            
            // получаем с бд пользователя с такой же почтой
            var user = await _userRepository.GetUserByEmail(userCreateDto.EMail);
            if (user == null)
            {
                _logger.LogError($"При попытке входа в аккаунт, пользователя с данной почтой не было обнаружено {userCreateDto.EMail}");

                return null;
            }

            // проверяем соответствие паролей
            if (BCrypt.Net.BCrypt.Verify(userCreateDto.Password, user.AuthorizationParams.PasswordHash))
            {
                return user;
            }
            
            _logger.LogError("Несоответствие паролей при попытке войти в аккаунт");
            return null;
        }
        public async Task<User?> SignIn (UserCreateDto userCreateDto)
        {
            _logger.LogInformation("Попытка зарегестрироваться");
            
            // проверка кода подтверждения
            if (!await CodeConfirmation(userCreateDto.Code, userCreateDto.EMail))
            {
                _logger.LogError("Неверный код подтверждения при попытке зарегестрироваться");
                throw new BadHttpRequestException("Неверный код");
            }

            // проверяем нет ли пользователй с таким же email В БД
            //если нету то создаем пользователя, сохраняем его в бд и возвращаем его,
            //а так же удаляем код подтверждения из базы данных, тк он уже не нужен
            if (await _userRepository.GetUserByEmail(userCreateDto.EMail) == null)
            {
                var user = userCreateDto.ToDomain();
                await _userRepository.Create(user);
                await _userConfirmationRepository.Delete(await _userConfirmationRepository.ReadByEmail(userCreateDto.EMail));
                return user;
            }

            _logger.LogError($"Неудачная попытка зарегистрироваться: Пользователь с email {userCreateDto.EMail} уже существует");
            throw new DuplicateNameException($"Пользователь с email {userCreateDto.EMail} уже существует");
        }

        public async Task<bool> CodeConfirmation(string token, string email)
        {
            _logger.LogInformation("Попытка сопостовления кода подтверждения с тем, что есть в базе данных");
            
            //ищем в бд коды подтверждения для пользователя с таким email
            //если не нашли такую почту или, почта с токеном не соответствуют тем что находятся в бд - возвращаем false
            var userConfirm = await _userConfirmationRepository.ReadByEmail(email);
            if (userConfirm == null ||
                userConfirm.Email != email ||
                userConfirm.Token != token)
            {
                _logger.LogInformation("Несоответствие кода подтверждения с тем, что есть в базе данных");
                
                return false;
            }

            return true;
        }
        /// <summary>
        /// Устанавливаем клаймы при входе в аккаунт
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        public async Task SetClaims(HttpContext context, Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userId.ToString())
            };

            var claimIdentity = new ClaimsIdentity(claims, "cookies");
            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimIdentity)
            );
        }

        /// <summary>
        /// Метод для обновления параметров авторизации авторизованного пользователя
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task UpdateAuthParams(HttpContext сontext, UserAuthParamsUpdateDto dto)
        {
            _logger.LogInformation("ПРобуем обновить параметры авторизации авторизованного пользователя");
            
            // получаем авторизованного пользователя
            var user = await _userRepository.GetUserByID(Guid.Parse(сontext.User.Identity.Name));


            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.AuthorizationParams.PasswordHash))
            {
                _logger.LogError(
                    "При попытке обновить авторизационные параметры авторизованного пользователя," +
                    " был неправельно введен старый пароль");
                throw new UnauthorizedAccessException();
            }

            //обновляем пользователя в бд
            user.ApplyUpdate(dto);
            await _userRepository.Update(user);
        }
    }
}

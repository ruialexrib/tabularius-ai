using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

/// <summary>Provides administrator-only application user management.</summary>
[Authorize(Roles = ApplicationRoles.Administrator)]
public sealed class UsersController(UserManager<ApplicationUser> userManager) : Controller
{
    /// <summary>Displays all application users and their current roles.</summary>
    /// <returns>The user administration view.</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users.OrderBy(user => user.DisplayName).ThenBy(user => user.UserName).ToListAsync();
        var model = new List<UserListItemViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                Role = roles.Contains(ApplicationRoles.Administrator) ? "Administrador" : "Utilizador",
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                CreatedAtUtc = user.CreatedAtUtc
            });
        }

        return View(model);
    }

    /// <summary>Displays the administrator form for creating a new application user.</summary>
    /// <returns>The user creation view.</returns>
    [HttpGet]
    public IActionResult Create() => View(new CreateUserViewModel());

    /// <summary>Creates a new application user and assigns the selected application role.</summary>
    /// <param name="model">The submitted user data.</param>
    /// <returns>The user list when successful; otherwise the creation view with validation errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ApplicationRoles.All.Contains(model.Role)) ModelState.AddModelError(nameof(model.Role), "Selecione um perfil válido.");
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.UserName.Trim(),
            DisplayName = model.DisplayName.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            LockoutEnabled = true
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, LocalizeIdentityError(error));
            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            foreach (var error in roleResult.Errors) ModelState.AddModelError(string.Empty, LocalizeIdentityError(error));
            return View(model);
        }

        TempData["SuccessMessage"] = $"Utilizador {user.UserName} criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Toggles the lock state of an application user while preventing the current administrator from locking their own account.</summary>
    /// <param name="id">The Identity user identifier.</param>
    /// <returns>The user administration view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser?.Id == user.Id)
        {
            TempData["ErrorMessage"] = "Não pode bloquear a conta com que iniciou sessão.";
            return RedirectToAction(nameof(Index));
        }

        var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
        var result = await userManager.SetLockoutEndDateAsync(user, isLocked ? null : DateTimeOffset.MaxValue);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
            ? isLocked ? $"Utilizador {user.UserName} desbloqueado." : $"Utilizador {user.UserName} bloqueado."
            : "Não foi possível alterar o estado do utilizador.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Converts an Identity error into a concise Portuguese user-facing message.</summary>
    /// <param name="error">The Identity validation error.</param>
    /// <returns>A localized validation message.</returns>
    private static string LocalizeIdentityError(IdentityError error) => error.Code switch
    {
        "DuplicateUserName" => "Já existe um utilizador com este nome.",
        "DuplicateEmail" => "Já existe um utilizador com este email.",
        "PasswordTooShort" => "A palavra-passe tem de ter pelo menos 12 caracteres.",
        "PasswordRequiresDigit" => "A palavra-passe tem de incluir pelo menos um algarismo.",
        "PasswordRequiresLower" => "A palavra-passe tem de incluir pelo menos uma letra minúscula.",
        "PasswordRequiresUpper" => "A palavra-passe tem de incluir pelo menos uma letra maiúscula.",
        "PasswordRequiresNonAlphanumeric" => "A palavra-passe tem de incluir pelo menos um carácter especial.",
        _ => "Não foi possível concluir a operação."
    };
}

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
    /// <summary>Displays a filtered and paginated list of application users.</summary>
    /// <param name="search">Optional free-text search across username, display name and email.</param>
    /// <param name="role">Optional application role filter.</param>
    /// <param name="status">Optional active or locked account state filter.</param>
    /// <param name="page">The requested one-based page number.</param>
    /// <param name="pageSize">The requested number of rows per page.</param>
    /// <returns>The user administration view.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? role, string? status, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page); pageSize = new[] { 10, 25, 50, 100 }.Contains(pageSize) ? pageSize : 10; search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(); role = ApplicationRoles.All.Contains(role ?? string.Empty) ? role : null; status = status is "active" or "locked" ? status : null;
        IQueryable<ApplicationUser> query = userManager.Users.AsNoTracking();
        if (search is not null) query = query.Where(user => (user.UserName != null && user.UserName.Contains(search)) || user.DisplayName.Contains(search) || (user.Email != null && user.Email.Contains(search)));
        var filteredUsers = await query.OrderBy(user => user.DisplayName).ThenBy(user => user.UserName).ToListAsync();
        var now = DateTimeOffset.UtcNow;
        if (status == "active") filteredUsers = filteredUsers.Where(user => !user.LockoutEnd.HasValue || user.LockoutEnd <= now).ToList();
        if (status == "locked") filteredUsers = filteredUsers.Where(user => user.LockoutEnd.HasValue && user.LockoutEnd > now).ToList();
        var rows = new List<UserListItemViewModel>(filteredUsers.Count);
        foreach (var user in filteredUsers) { var roles = await userManager.GetRolesAsync(user); if (role is not null && !roles.Contains(role)) continue; rows.Add(new UserListItemViewModel { Id = user.Id, UserName = user.UserName ?? string.Empty, DisplayName = user.DisplayName, Email = user.Email ?? string.Empty, Role = roles.Contains(ApplicationRoles.Administrator) ? "Administrador" : "Utilizador", IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > now, CreatedAtUtc = user.CreatedAtUtc }); }
        var totalItems = rows.Count; var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize)); page = Math.Min(page, totalPages);
        return View(new UserListViewModel { Role = role, Status = status, List = new PagedListViewModel<UserListItemViewModel> { Items = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList(), TotalItems = totalItems, Page = page, PageSize = pageSize, Search = search } });
    }

    /// <summary>Displays the administrator form for creating a new application user.</summary>
    /// <returns>The user creation view.</returns>
    [HttpGet] public IActionResult Create() => View(new CreateUserViewModel());

    /// <summary>Creates a new application user and assigns the selected application role.</summary>
    /// <param name="model">The submitted user data.</param><returns>The user list when successful; otherwise the creation view.</returns>
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ApplicationRoles.All.Contains(model.Role)) ModelState.AddModelError(nameof(model.Role), "Selecione um perfil válido."); if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.UserName.Trim(), DisplayName = model.DisplayName.Trim(), Email = model.Email.Trim(), EmailConfirmed = true, LockoutEnabled = true }; var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) { AddIdentityErrors(result); return View(model); } var roleResult = await userManager.AddToRoleAsync(user, model.Role); if (!roleResult.Succeeded) { await userManager.DeleteAsync(user); AddIdentityErrors(roleResult); return View(model); }
        TempData["SuccessMessage"] = $"Utilizador {user.UserName} criado com sucesso."; return RedirectToAction(nameof(Index));
    }

    /// <summary>Displays the administrator form for editing an existing application user.</summary>
    /// <param name="id">The Identity user identifier.</param><returns>The user editing view, or not found.</returns>
    [HttpGet] public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id); if (user is null) return NotFound(); var roles = await userManager.GetRolesAsync(user);
        return View(new EditUserViewModel { Id = user.Id, UserName = user.UserName ?? string.Empty, DisplayName = user.DisplayName, Email = user.Email ?? string.Empty, Role = roles.Contains(ApplicationRoles.Administrator) ? ApplicationRoles.Administrator : ApplicationRoles.User });
    }

    /// <summary>Updates an existing application user's identity data and application role.</summary>
    /// <param name="model">The submitted user data.</param><returns>The user list when successful; otherwise the editing view.</returns>
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ApplicationRoles.All.Contains(model.Role)) ModelState.AddModelError(nameof(model.Role), "Selecione um perfil válido."); var user = await userManager.FindByIdAsync(model.Id); if (user is null) return NotFound(); var currentUser = await userManager.GetUserAsync(User); var currentRoles = await userManager.GetRolesAsync(user);
        if (currentUser?.Id == user.Id && currentRoles.Contains(ApplicationRoles.Administrator) && model.Role != ApplicationRoles.Administrator) ModelState.AddModelError(nameof(model.Role), "Não pode remover o perfil de Administrador da conta com que iniciou sessão."); if (!ModelState.IsValid) return View(model);
        user.DisplayName = model.DisplayName.Trim(); var userNameResult = await userManager.SetUserNameAsync(user, model.UserName.Trim()); if (!userNameResult.Succeeded) { AddIdentityErrors(userNameResult); return View(model); } var emailResult = await userManager.SetEmailAsync(user, model.Email.Trim()); if (!emailResult.Succeeded) { AddIdentityErrors(emailResult); return View(model); } user.EmailConfirmed = true; var updateResult = await userManager.UpdateAsync(user); if (!updateResult.Succeeded) { AddIdentityErrors(updateResult); return View(model); }
        if (!currentRoles.Contains(model.Role)) { var removableRoles = currentRoles.Where(ApplicationRoles.All.Contains).ToArray(); if (removableRoles.Length > 0) { var removeResult = await userManager.RemoveFromRolesAsync(user, removableRoles); if (!removeResult.Succeeded) { AddIdentityErrors(removeResult); return View(model); } } var addResult = await userManager.AddToRoleAsync(user, model.Role); if (!addResult.Succeeded) { AddIdentityErrors(addResult); return View(model); } }
        TempData["SuccessMessage"] = $"Utilizador {user.UserName} atualizado com sucesso."; return RedirectToAction(nameof(Index));
    }

    /// <summary>Displays the administrator form for resetting an application user's password.</summary>
    /// <param name="id">The Identity user identifier.</param><returns>The password reset view, or not found.</returns>
    [HttpGet] public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await userManager.FindByIdAsync(id); if (user is null) return NotFound();
        return View(new ResetUserPasswordViewModel { Id = user.Id, UserName = user.UserName ?? string.Empty, DisplayName = user.DisplayName });
    }

    /// <summary>Resets an application user's password using an Identity reset token.</summary>
    /// <param name="model">The submitted password reset data.</param><returns>The user list when successful; otherwise the reset view.</returns>
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ResetPassword(ResetUserPasswordViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.Id); if (user is null) return NotFound(); model.UserName = user.UserName ?? string.Empty; model.DisplayName = user.DisplayName; if (!ModelState.IsValid) return View(model);
        var token = await userManager.GeneratePasswordResetTokenAsync(user); var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword); if (!result.Succeeded) { AddIdentityErrors(result); return View(model); }
        await userManager.UpdateSecurityStampAsync(user); TempData["SuccessMessage"] = $"Palavra-passe de {user.UserName} redefinida com sucesso."; return RedirectToAction(nameof(Index));
    }

    /// <summary>Toggles the lock state of an application user while preventing the current administrator from locking their own account.</summary>
    /// <param name="id">The Identity user identifier.</param><returns>The user administration view.</returns>
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await userManager.FindByIdAsync(id); if (user is null) return NotFound(); var currentUser = await userManager.GetUserAsync(User); if (currentUser?.Id == user.Id) { TempData["ErrorMessage"] = "Não pode bloquear a conta com que iniciou sessão."; return RedirectToAction(nameof(Index)); }
        var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow; var result = await userManager.SetLockoutEndDateAsync(user, isLocked ? null : DateTimeOffset.MaxValue); TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? isLocked ? $"Utilizador {user.UserName} desbloqueado." : $"Utilizador {user.UserName} bloqueado." : "Não foi possível alterar o estado do utilizador."; return RedirectToAction(nameof(Index));
    }

    /// <summary>Adds localized Identity errors to the current model state.</summary>
    /// <param name="result">The failed Identity operation result.</param>
    private void AddIdentityErrors(IdentityResult result) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, LocalizeIdentityError(error)); }

    /// <summary>Converts an Identity error into a concise Portuguese user-facing message.</summary>
    /// <param name="error">The Identity validation error.</param><returns>A localized validation message.</returns>
    private static string LocalizeIdentityError(IdentityError error) => error.Code switch { "DuplicateUserName" => "Já existe um utilizador com este nome.", "DuplicateEmail" => "Já existe um utilizador com este email.", "PasswordTooShort" => "A palavra-passe tem de ter pelo menos 12 caracteres.", "PasswordRequiresDigit" => "A palavra-passe tem de incluir pelo menos um algarismo.", "PasswordRequiresLower" => "A palavra-passe tem de incluir pelo menos uma letra minúscula.", "PasswordRequiresUpper" => "A palavra-passe tem de incluir pelo menos uma letra maiúscula.", "PasswordRequiresNonAlphanumeric" => "A palavra-passe tem de incluir pelo menos um carácter especial.", _ => "Não foi possível concluir a operação." };
}

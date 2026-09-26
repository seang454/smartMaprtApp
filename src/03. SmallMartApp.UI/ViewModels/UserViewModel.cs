using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.UI.Services;

namespace SmallMartApp.UI.ViewModels;

public partial class UserViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly List<User> _allUsersCache = new();
    private bool _isLoading;

    public ObservableCollection<User> Users { get; } = new();

    // --- KPI Statistics ---
    [ObservableProperty]
    private int _totalStaffCount;

    [ObservableProperty]
    private int _adminCount;

    [ObservableProperty]
    private int _cashierCount;

    [ObservableProperty]
    private int _activeStaffCount;

    // --- Search, Filter & Sort Controls ---
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedRoleFilter = "All Roles";

    public List<string> RoleFilters { get; } = new()
    {
        "All Roles",
        "Admin",
        "Cashier"
    };

    [ObservableProperty]
    private string _selectedShiftFilter = "All Shifts";

    public List<string> ShiftFilters { get; } = new()
    {
        "All Shifts",
        "Morning",
        "Afternoon",
        "Night",
        "FullTime"
    };

    [ObservableProperty]
    private string _selectedStatusFilter = "All Status";

    public List<string> StatusFilters { get; } = new()
    {
        "All Status",
        "Active Only",
        "Inactive Only"
    };

    [ObservableProperty]
    private string _sortBy = "Name (A - Z)";

    public List<string> SortOptions { get; } = new()
    {
        "Name (A - Z)",
        "Name (Z - A)",
        "Username (A - Z)",
        "Role (Admin First)",
        "Role (Cashier First)",
        "Newest First",
        "Oldest First"
    };

    // --- Pagination Controls ---
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalFilteredCount = 0;

    [ObservableProperty]
    private string _paginationInfo = string.Empty;

    [ObservableProperty]
    private string _pageDisplay = "Page 1 of 1";

    public List<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50 };

    public bool CanPreviousPage => CurrentPage > 1;
    public bool CanNextPage => CurrentPage < TotalPages;

    [RelayCommand]
    public void FirstPage()
    {
        if (CurrentPage != 1)
        {
            CurrentPage = 1;
            ApplyFiltersAndSort();
        }
    }

    [RelayCommand]
    public void PreviousPage()
    {
        if (CanPreviousPage)
        {
            CurrentPage--;
            ApplyFiltersAndSort();
        }
    }

    [RelayCommand]
    public void NextPage()
    {
        if (CanNextPage)
        {
            CurrentPage++;
            ApplyFiltersAndSort();
        }
    }

    [RelayCommand]
    public void LastPage()
    {
        if (CurrentPage != TotalPages)
        {
            CurrentPage = TotalPages;
            ApplyFiltersAndSort();
        }
    }

    // --- Role and Shift Options for Creation / Editing ---
    public List<UserRole> AvailableRoles { get; } = new()
    {
        UserRole.Cashier,
        UserRole.Admin
    };

    public List<string> AvailableShifts { get; } = new()
    {
        "Morning",
        "Afternoon",
        "Night",
        "FullTime"
    };

    // --- Create User Form Fields ---
    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _newFullName = string.Empty;

    [ObservableProperty]
    private UserRole _newRole = UserRole.Cashier;

    [ObservableProperty]
    private string _newWorkingShift = "Morning";

    // --- Edit User Modal Properties ---
    [ObservableProperty]
    private bool _isEditUserOpen;

    [ObservableProperty]
    private int _editUserId;

    [ObservableProperty]
    private string _editUsername = string.Empty;

    [ObservableProperty]
    private string _editFullName = string.Empty;

    [ObservableProperty]
    private UserRole _editRole = UserRole.Cashier;

    [ObservableProperty]
    private string _editWorkingShift = "Morning";

    [ObservableProperty]
    private bool _editIsActive = true;

    [ObservableProperty]
    private string _editNewPassword = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public UserViewModel(
        IUserService userService,
        INotificationService notificationService)
    {
        _userService = userService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var items = await _userService.GetAllAsync();
            _allUsersCache.Clear();
            _allUsersCache.AddRange(items);

            UpdateKpis();
            ApplyFiltersAndSort();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void UpdateKpis()
    {
        TotalStaffCount = _allUsersCache.Count;
        AdminCount = _allUsersCache.Count(u => u.Role == UserRole.Admin);
        CashierCount = _allUsersCache.Count(u => u.Role == UserRole.Cashier);
        ActiveStaffCount = _allUsersCache.Count(u => u.IsActive);
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allUsersCache.AsEnumerable();

        // 1. Search Query (Username or Full Name)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string query = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(u =>
                (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLowerInvariant().Contains(query)) ||
                (!string.IsNullOrEmpty(u.Username) && u.Username.ToLowerInvariant().Contains(query)));
        }

        // 2. Role Filter
        filtered = SelectedRoleFilter switch
        {
            "Admin" => filtered.Where(u => u.Role == UserRole.Admin),
            "Cashier" => filtered.Where(u => u.Role == UserRole.Cashier),
            _ => filtered
        };

        // 3. Shift Filter
        if (SelectedShiftFilter != "All Shifts")
        {
            filtered = filtered.Where(u => string.Equals(u.WorkingShift, SelectedShiftFilter, StringComparison.OrdinalIgnoreCase));
        }

        // 4. Status Filter
        filtered = SelectedStatusFilter switch
        {
            "Active Only" => filtered.Where(u => u.IsActive),
            "Inactive Only" => filtered.Where(u => !u.IsActive),
            _ => filtered
        };

        // 5. Sorting
        filtered = SortBy switch
        {
            "Name (A - Z)" => filtered.OrderBy(u => u.FullName),
            "Name (Z - A)" => filtered.OrderByDescending(u => u.FullName),
            "Username (A - Z)" => filtered.OrderBy(u => u.Username),
            "Role (Admin First)" => filtered.OrderBy(u => u.Role).ThenBy(u => u.FullName),
            "Role (Cashier First)" => filtered.OrderByDescending(u => u.Role).ThenBy(u => u.FullName),
            "Newest First" => filtered.OrderByDescending(u => u.Id),
            "Oldest First" => filtered.OrderBy(u => u.Id),
            _ => filtered.OrderBy(u => u.FullName)
        };

        var filteredList = filtered.ToList();
        TotalFilteredCount = filteredList.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalFilteredCount / PageSize));

        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        var pagedItems = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Users.Clear();
        foreach (var item in pagedItems)
        {
            Users.Add(item);
        }

        int start = TotalFilteredCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        int end = Math.Min(CurrentPage * PageSize, TotalFilteredCount);
        PaginationInfo = TotalFilteredCount == 0
            ? "No staff members found"
            : $"Showing {start}-{end} of {TotalFilteredCount} staff members";
        PageDisplay = $"Page {CurrentPage} of {TotalPages}";

        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
    }

    [RelayCommand]
    public void ResetFilters()
    {
        CurrentPage = 1;
        SearchText = string.Empty;
        SelectedRoleFilter = "All Roles";
        SelectedShiftFilter = "All Shifts";
        SelectedStatusFilter = "All Status";
        SortBy = "Name (A - Z)";
        ApplyFiltersAndSort();
    }

    // --- CRUD: CREATE ---
    [RelayCommand]
    public async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewFullName) || string.IsNullOrWhiteSpace(NewPassword))
        {
            StatusMessage = "Username, Full Name, and Password are all required.";
            _notificationService.ShowWarning("Username, Full Name, and Password are all required.", "Validation");
            return;
        }

        var user = new User
        {
            Username = NewUsername.Trim(),
            FullName = NewFullName.Trim(),
            Role = NewRole,
            WorkingShift = NewWorkingShift,
            IsActive = true
        };

        var res = await _userService.AddOrUpdateAsync(user, NewPassword.Trim());
        if (res.IsSuccess)
        {
            StatusMessage = $"Staff member '{user.FullName}' ({user.Role}) added successfully!";
            _notificationService.ShowSuccess($"Staff member '{user.FullName}' registered successfully!");

            NewUsername = string.Empty;
            NewPassword = string.Empty;
            NewFullName = string.Empty;
            NewRole = UserRole.Cashier;
            NewWorkingShift = "Morning";

            await LoadUsersAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Error creating user.";
            _notificationService.ShowError(res.Error ?? "Error creating user.");
        }
    }

    // --- CRUD: UPDATE (EDIT MODAL) ---
    [RelayCommand]
    public void OpenEditUser(User user)
    {
        if (user == null) return;
        EditUserId = user.Id;
        EditUsername = user.Username;
        EditFullName = user.FullName;
        EditRole = user.Role;
        EditWorkingShift = user.WorkingShift;
        EditIsActive = user.IsActive;
        EditNewPassword = string.Empty; // Blank means do not change
        IsEditUserOpen = true;
    }

    [RelayCommand]
    public void CloseEditUser()
    {
        IsEditUserOpen = false;
        EditNewPassword = string.Empty;
    }

    [RelayCommand]
    public async Task SaveEditUserAsync()
    {
        if (string.IsNullOrWhiteSpace(EditUsername) || string.IsNullOrWhiteSpace(EditFullName))
        {
            StatusMessage = "Username and Full Name are required.";
            _notificationService.ShowWarning("Username and Full Name are required.", "Validation");
            return;
        }

        var user = new User
        {
            Id = EditUserId,
            Username = EditUsername.Trim(),
            FullName = EditFullName.Trim(),
            Role = EditRole,
            WorkingShift = EditWorkingShift,
            IsActive = EditIsActive
        };

        string? newPassword = string.IsNullOrWhiteSpace(EditNewPassword) ? null : EditNewPassword.Trim();
        var res = await _userService.AddOrUpdateAsync(user, newPassword);

        if (!res.IsSuccess)
        {
            StatusMessage = res.Error ?? "Error updating staff member.";
            _notificationService.ShowError(res.Error ?? "Error updating staff member.");
            return;
        }

        IsEditUserOpen = false;
        EditNewPassword = string.Empty;
        StatusMessage = $"Updated staff member '{user.FullName}' successfully!";
        _notificationService.ShowSuccess($"Staff member '{user.FullName}' updated successfully!");

        await LoadUsersAsync();
    }

    // --- CRUD: TOGGLE ACTIVE STATUS ---
    [RelayCommand]
    public async Task ToggleUserActiveAsync(User user)
    {
        if (user == null) return;

        var res = await _userService.ToggleActiveAsync(user.Id);
        if (res.IsSuccess)
        {
            user.IsActive = !user.IsActive;
            string state = user.IsActive ? "Activated" : "Deactivated";
            StatusMessage = $"{state} user '{user.Username}'.";
            _notificationService.ShowSuccess($"User '{user.Username}' is now {state.ToLowerInvariant()}.");
            UpdateKpis();
            ApplyFiltersAndSort();
        }
        else
        {
            StatusMessage = res.Error ?? $"Could not toggle status for '{user.Username}'.";
            _notificationService.ShowError(res.Error ?? $"Could not toggle status for '{user.Username}'.");
        }
    }

    // --- CRUD: DELETE ---
    [RelayCommand]
    public async Task DeleteUserAsync(User user)
    {
        if (user == null) return;

        var res = await _userService.DeleteAsync(user.Id);
        if (res.IsSuccess)
        {
            _allUsersCache.RemoveAll(u => u.Id == user.Id);
            Users.Remove(user);
            StatusMessage = $"Removed user '{user.Username}'.";
            _notificationService.ShowSuccess($"Staff member '{user.Username}' removed successfully!");
            UpdateKpis();
            ApplyFiltersAndSort();
        }
        else
        {
            StatusMessage = res.Error ?? $"Could not delete '{user.Username}'.";
            // If it failed because user has shift audits, it was deactivated
            _notificationService.ShowWarning(res.Error ?? $"Could not delete '{user.Username}'.", "Staff Audit Protection");
            await LoadUsersAsync();
        }
    }

    partial void OnSearchTextChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedRoleFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedShiftFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedStatusFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSortByChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFiltersAndSort(); }
}

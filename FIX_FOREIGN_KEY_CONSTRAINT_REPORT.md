# Fix: Foreign Key Constraint Error - ReportedProviderId

## Problem
```
SqlException: The INSERT statement conflicted with the FOREIGN KEY constraint 
"FK_Reports_Clients_ReportedProviderId". The conflict occurred in database 
"LebAssistDatabase", table "dbo.Clients", column 'ClientId'.
```

### Root Cause
The application was attempting to insert a `Report` with a `ReportedProviderId` that doesn't exist in the `Clients` table. This happens when:
1. Invalid provider ID is passed to the report creation
2. Provider ID from form data doesn't exist in the database
3. Client tries to report a non-provider user
4. Booking data is inconsistent

## Solution Implemented

### 1. **Service Layer Validation** (`ReportService.cs`)

Added comprehensive validation in `CreateReportAsync`:

```csharp
// Validate that the reported provider exists
var provider = await _unitOfWork.Clients.GetByIdAsync(dto.ReportedProviderId);
if (provider == null)
{
    throw new ArgumentException($"Provider with ID {dto.ReportedProviderId} does not exist.");
}

// Validate that the provider is actually a provider
if (!provider.IsProvider)
{
    throw new ArgumentException($"Client {dto.ReportedProviderId} is not a provider.");
}

// Validate booking if provided
if (dto.BookingId.HasValue)
{
    var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId.Value);
    if (booking == null)
    {
        throw new ArgumentException($"Booking with ID {dto.BookingId.Value} does not exist.");
    }

    // Verify the booking belongs to the client
    if (booking.ClientId != clientId)
    {
        throw new UnauthorizedAccessException("You can only report your own bookings.");
    }

    // Verify the booking provider matches the reported provider
    if (booking.ProviderId != dto.ReportedProviderId)
    {
        throw new ArgumentException("The reported provider must match the booking's provider.");
    }
}
```

### 2. **Controller Layer Error Handling** (`ReportController.cs`)

#### Updated POST Create Action:
```csharp
try
{
    var dto = new CreateReportDto
    {
        ReportedProviderId = model.ProviderId,
        BookingId = model.BookingId,
        Reason = model.Reason,
        Description = model.Description
    };

    var reportId = await _reportService.CreateReportAsync(profile.ClientId, dto);
    TempData["Success"] = "Your report has been submitted successfully.";
    return RedirectToAction("Details", new { id = reportId });
}
catch (ArgumentException ex)
{
    TempData["Error"] = ex.Message;
    return View(model);
}
catch (UnauthorizedAccessException ex)
{
    TempData["Error"] = ex.Message;
    return RedirectToAction("MyReports");
}
catch (Exception)
{
    TempData["Error"] = "An error occurred while submitting your report.";
    return View(model);
}
```

#### Enhanced GET Create Action:
```csharp
// Support both bookingId and direct providerId
public async Task<IActionResult> Create(int? bookingId, int? providerId)
{
    // ... authentication checks ...

    if (bookingId.HasValue)
    {
        // Validate booking and auto-populate provider info
        var booking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
        if (booking != null && booking.ClientId == profile.ClientId)
        {
            model.ProviderId = booking.ProviderId;
            model.ProviderName = booking.ProviderName;
            model.ServiceName = booking.ServiceName;
            model.BookingDate = booking.ScheduledDateTime;
        }
        else
        {
            TempData["Error"] = "Booking not found or unauthorized.";
            return RedirectToAction("MyReports");
        }
    }
    else if (providerId.HasValue)
    {
        // Validate provider directly
        var provider = await _clientService.GetClientByIdAsync(providerId.Value);
        if (provider != null && provider.IsProvider)
        {
            model.ProviderId = providerId.Value;
            model.ProviderName = $"{provider.FirstName} {provider.LastName}";
        }
        else
        {
            TempData["Error"] = "Provider not found or invalid.";
            return RedirectToAction("MyReports");
        }
    }
    else
    {
        TempData["Error"] = "Please specify either a booking or a provider to report.";
        return RedirectToAction("MyReports");
    }
}
```

## Validation Checks Added

### Provider Validation ?
1. **Provider Exists**: Checks if the provider ID exists in the database
2. **Is Provider**: Verifies the client is actually a provider (not just a regular client)

### Booking Validation ?
3. **Booking Exists**: Verifies the booking ID is valid
4. **Booking Ownership**: Ensures the client owns the booking they're reporting
5. **Provider Match**: Confirms the reported provider matches the booking's provider

### Authorization ?
6. **User Authentication**: Verifies user is logged in
7. **Profile Access**: Confirms user has a valid profile
8. **Access Rights**: Ensures users can only report their own bookings

## User Experience Improvements

### Error Messages
Users now get clear, specific error messages:
- ? "Provider with ID X does not exist."
- ? "Client X is not a provider."
- ? "Booking with ID X does not exist."
- ? "You can only report your own bookings."
- ? "The reported provider must match the booking's provider."
- ? "Please specify either a booking or a provider to report."

### Graceful Degradation
Instead of SQL exceptions, users see:
- Friendly error messages
- Stay on the form to fix issues
- Redirected to safe pages when needed

## Usage Examples

### Report from Booking
```url
/Report/Create?bookingId=5
```
? Auto-populates provider info from booking
? Validates booking ownership
? Validates provider exists

### Report Provider Directly
```url
/Report/Create?providerId=10
```
? Validates provider exists
? Validates provider is actually a provider
? Allows reporting without a booking

## Testing Checklist

### Test Cases
- [ ] Create report with valid booking ID
- [ ] Create report with invalid booking ID
- [ ] Create report with someone else's booking
- [ ] Create report with valid provider ID
- [ ] Create report with invalid provider ID
- [ ] Create report with regular client ID (not provider)
- [ ] Create report with non-existent provider
- [ ] Create report without booking or provider

### Expected Results
? Valid reports are created successfully
? Invalid attempts show appropriate error messages
? No SQL exceptions reach the user
? Database integrity is maintained

## Database Constraints

The fix ensures these constraints are always met:
```sql
FK_Reports_Clients_ReporterId          -- Reporter must exist
FK_Reports_Clients_ReportedProviderId  -- Provider must exist ? FIXED
FK_Reports_Bookings_BookingId          -- Booking must exist (if provided)
```

## Benefits

1. **Data Integrity**: Prevents orphaned foreign key references
2. **User Experience**: Clear error messages instead of exceptions
3. **Security**: Prevents unauthorized reporting
4. **Reliability**: Catches errors before database operations
5. **Maintainability**: Centralized validation logic

## Related Files Modified

1. `LebAssist.Application\Services\ReportService.cs` - Added validation logic
2. `LebAssist.Presentation\Controllers\ReportController.cs` - Added error handling

## Build Status
? **Build Successful** - All changes compile without errors

---

**Status**: ? **Fixed**  
**Root Cause**: Missing validation before database insert  
**Solution**: Added comprehensive validation at service layer  
**User Impact**: Friendly error messages instead of SQL exceptions

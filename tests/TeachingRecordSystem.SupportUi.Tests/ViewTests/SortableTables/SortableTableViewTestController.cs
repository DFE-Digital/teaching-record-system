using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.SortableTables;

public class SortableTableViewTestController : Controller
{
    [HttpGet("_sortable-table")]
    public IActionResult SortableTable([FromQuery] SortDirection? sortDirection) =>
        View("/ViewTests/SortableTables/SortableTable.cshtml", sortDirection);
}

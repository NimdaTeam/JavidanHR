// ContractDto.cs
using PayrollSystem.Domain.Entities.Contract;
using System.ComponentModel.DataAnnotations;

namespace PayrollSystem.Application.DTOs
{
    public class ContractPayItemInputDto
    {
        // For database pay items
        public long? PayItemId { get; set; }

        // For system pay items
        public string? SystemCode { get; set; }

        // Value for user-input items
        public decimal? Value { get; set; }

        /// <summary>
        /// Indicates whether this is a system pay item
        /// </summary>
        public bool IsSystemItem => !string.IsNullOrWhiteSpace(SystemCode);
    }

    /// <summary>
    /// Data transfer object for Contract.
    /// </summary>
    public class ContractDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string? EmployeeName { get; set; } 
        public long WorkshopId { get; set; }
        public string? WorkshopName { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public bool IsActive { get; set; }
        public ContractStatus? Status { get; set; }
        public List<ContractPayItemDto> PayItems { get; set; } = [];
    }

    /// <summary>
    /// DTO for contract pay item (supports both database and system items)
    /// </summary>
    public class ContractPayItemDto
    {
        public long Id { get; set; }
        // For database pay items
        public long? PayItemId { get; set; }

        // For system pay items
        public string? SystemCode { get; set; }

        // Indicates if this is a system item
        public bool IsSystemItem { get; set; }

        // Display name (from database or system constants)
        public string? PayItemName { get; set; } = string.Empty;

        // Value for user-input items
        public decimal? Value { get; set; }
    }

    /// <summary>
    /// Command to create a new contract.
    /// </summary>
    public class CreateContractDto
    {
        public long EmployeeId { get; set; }
        public long WorkshopId { get; set; }
        public string ValidFromDateJalali { get; set; }
        public string? ValidToDateJalali { get; set; }
        public List<ContractPayItemInputDto> PayItems { get; set; } = new();
    }

    /// <summary>
    /// Command to assign a pay item to a contract.
    /// </summary>
    public class AssignPayItemToContractDto
    {
        public long ContractId { get; set; }
        public long PayItemId { get; set; }
        public decimal? Value { get; set; } // optional for UserInput items
    }


	public class EditContractDto
	{
		public long Id { get; set; }
		public long EmployeeId { get; set; }
		public long WorkshopId { get; set; }
		public string ValidFromDateJalali { get; set; }
		public string? ValidToDateJalali { get; set; }
		public List<EditPayItemDto> PayItems { get; set; } = new();
	}

	public class EditPayItemDto
	{
		public long? Id { get; set; } // Id of ContractPayItem (for updates/deletes)
		public long? PayItemId { get; set; } // For database items
		public string? SystemCode { get; set; } // For system items
		public bool IsSystemItem => PayItemId == null && !string.IsNullOrEmpty(SystemCode);
		public decimal? Value { get; set; }
		public bool IsDeleted { get; set; } // Mark for removal
	}
}



using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FSpot.Models
{
	public partial class Job : BaseModel
	{
		// TODO, Is the column Id being used as an index?
		//		 ie, the order in which the jobs should be run?
		[NotMapped]
		public long OldId { get; set; }
		public string JobType { get; set; }
		public string JobOptions { get; set; }
		public long? RunAt { get; set; }
		public long JobPriority { get; set; }
	}
}

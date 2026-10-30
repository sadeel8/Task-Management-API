namespace Task_Management_API.DTO.Tasks
{
    public class SaveTaskDto
    {
        public long? Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long? StatusId { get; set; }
    }
}

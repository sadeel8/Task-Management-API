using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Management_API.DbContexts;
using Task_Management_API.DTO.Tasks;
using Task_Management_API.Models;

namespace Task_Management_API.Controllers
{
    [Authorize]
    [Route("api/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagementContext _dbContext;

        public TasksController(TaskManagementContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetByCriteria")]
        public IActionResult GetByCriteria([FromQuery] SearchTaskDto searchTaskDto)
        {
            try
            {
                //Extracted From Token
                var userId = long.Parse(User.FindFirst("UserId")?.Value!);

                var data = from task in _dbContext.Tasks
                           from lookup in _dbContext.Lookups.Where(x => x.Id == task.StatusId).DefaultIfEmpty()
                           where task.UserId == userId &&
                           (searchTaskDto.StatusId == null || task.StatusId == searchTaskDto.StatusId) &&
                           (searchTaskDto.Title == null || task.Title.Contains(searchTaskDto.Title))
                           orderby task.Id
                           select new TaskDto
                           {
                               Id = task.Id,
                               Title = task.Title,
                               Description = task.Description,
                               FromDate = task.FromDate,
                               ToDate = task.ToDate,
                               StatusId = task.StatusId,
                               StatusName = lookup.Name,
                               UserId = task.UserId
                           };

                return Ok(data.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception(ex.Message));
            }
        }

        [HttpGet("GetById/{id:long}")]//Rout Parameter
        public IActionResult GetById(long id)//ترجع مهمة واحدة فقط
        {
            try
            {
                var userId = long.Parse(User.FindFirst("UserId")?.Value!);

                var task = _dbContext.Tasks.Include(x => x.Status).FirstOrDefault(x => x.Id == id);
                if (task == null)
                {
                    return NotFound(new Exception("Task not found"));
                }
                if (task.UserId != userId)
                {
                    return BadRequest(new Exception("Task is assigned to another user."));
                }
                return Ok(new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    FromDate = task.FromDate,
                    ToDate = task.ToDate,
                    StatusId = task.StatusId,
                    StatusName = task.Status?.Name,
                    UserId = task.UserId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception(ex.Message));
            }
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] SaveTaskDto taskDto)// انشاء مهمة جديدة
        {
            try
            {
                var userId = long.Parse(User.FindFirst("UserId")?.Value!);

                if (taskDto.FromDate > taskDto.ToDate)
                {
                    return BadRequest(new Exception("FromDate must be earlier than or equal to ToDate."));
                }
                if (taskDto.StatusId != null && !_dbContext.Lookups.Any(x => x.Id == taskDto.StatusId))
                {
                    return BadRequest(new Exception("Invalid StatusId."));
                }

                var task = new Models.Task()
                {
                    Id = 0,
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    FromDate = taskDto.FromDate,
                    ToDate = taskDto.ToDate,
                    StatusId = taskDto.StatusId,
                    UserId = userId
                };
                _dbContext.Tasks.Add(task);
                _dbContext.SaveChanges(); //Go's to database
                _dbContext.Entry(task).Reference(x => x.Status).Load();

                return CreatedAtAction(nameof(GetById), new { id = task.Id }, new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    FromDate = task.FromDate,
                    ToDate = task.ToDate,
                    StatusId = task.StatusId,
                    StatusName = task.Status?.Name,
                    UserId = task.UserId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception(ex.Message));
            }
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] SaveTaskDto taskDto)
        {
            try
            {
                var userId = long.Parse(User.FindFirst("UserId")?.Value!);

                if (taskDto.Id == null)
                {
                    return BadRequest(new Exception("Id is required."));
                }
                if (taskDto.FromDate > taskDto.ToDate)
                {
                    return BadRequest(new Exception("FromDate must be earlier than or equal to ToDate."));
                }
                var task = _dbContext.Tasks.FirstOrDefault(x => x.Id == taskDto.Id);
                if (task == null)
                {
                    return NotFound(new Exception("Task not found"));
                }
                if (task.UserId != userId)
                {
                    return BadRequest(new Exception("Task is assigned to another user."));
                }
                if (taskDto.StatusId != null && !_dbContext.Lookups.Any(x => x.Id == taskDto.StatusId))
                {
                    return BadRequest(new Exception("Invalid StatusId."));
                }

                task.Title = taskDto.Title;
                task.Description = taskDto.Description;
                task.FromDate = taskDto.FromDate;
                task.ToDate = taskDto.ToDate;
                task.StatusId = taskDto.StatusId;

                _dbContext.SaveChanges();
                _dbContext.Entry(task).Reference(x => x.Status).Load();
                return Ok(new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    FromDate = task.FromDate,
                    ToDate = task.ToDate,
                    StatusId = task.StatusId,
                    StatusName = task.Status?.Name,
                    UserId = task.UserId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception(ex.Message));
            }
        }

        [HttpDelete("Delete/{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                var userId = long.Parse(User.FindFirst("UserId")?.Value!);

                var task = _dbContext.Tasks.FirstOrDefault(x => x.Id == id);
                if (task == null)
                {
                    return NotFound(new Exception("Task not found"));
                }
                if (task.UserId != userId)
                {
                    return BadRequest(new Exception("Task is assigned to another user."));
                }
                _dbContext.Tasks.Remove(task);
                _dbContext.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception(ex.Message));
            }
        }
    }
}

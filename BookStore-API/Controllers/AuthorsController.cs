using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository authorRepository;
        private readonly ILoggerService logger;
        private readonly IMapper mapper;

        private ObjectResult InternalError(string message)
        {
            logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }

        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            this.authorRepository = authorRepository;
            this.logger = logger;
            this.mapper = mapper;
        }


        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }



        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location} : Executed");
                var authors = await authorRepository.FindAll();
                var response = mapper.Map<IList<AuthorDTO>>(authors);
                logger.LogInfo($"{location}: Successfull");

                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get Author by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>An Author Record</returns>
        [HttpGet("{Id:int}")]
        [Authorize(Roles ="Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int Id)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Executed with Id:{Id}");
                var author = await authorRepository.FindById(Id);

                if (author == null)
                {
                    logger.LogWarn($"{location}: Id:{Id} was not found");
                    return NotFound();
                }
                var response = mapper.Map<AuthorDTO>(author);
                logger.LogInfo($"{location}: Successfull with Id:{Id}");

                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Create Attempted");
                if(authorDTO == null)
                {
                    logger.LogWarn($"{location}: Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    logger.LogWarn($"{location}: Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var author = mapper.Map<Author>(authorDTO);
                var isSuccess = await authorRepository.Create(author);

                if (!isSuccess)
                {
                    return InternalError($"{location}: Creation failed");
                }
                logger.LogInfo($"{location}: Creation was Successful");
                logger.LogInfo($"{location}: {author}");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{Id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int Id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Update Attempted on record with id:{Id}");
                if (Id < 1 || authorDTO == null || Id != authorDTO.Id)
                {
                    logger.LogWarn($"{location}: Update with bad data - id: {Id}");
                    return BadRequest();
                }

                var isExists = await authorRepository.isExists(Id);
                if (!isExists)
                {
                    logger.LogWarn($"{location}: Failed to retrieve record with id: {Id}");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    logger.LogWarn($"{location}: Data was Incomplete");
                    return BadRequest(ModelState);
                }

                var author = mapper.Map<Author>(authorDTO);
                var isSuccess = await authorRepository.Update(author);

                if (!isSuccess)
                {
                    return InternalError($"{location}: Update Failed");
                }
                logger.LogWarn($"{location}: Record with id: {Id} successfulyy updated");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Removes an author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// [HttpDelete("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: id: {id} Delete Attempted");
                if(id < 1)
                {
                    logger.LogInfo($"{location}: Delete Failed with bad data");
                    return BadRequest();
                }

                var isExists = await authorRepository.isExists(id);
                if (!isExists)
                {
                    logger.LogWarn($"{location}: id:{id} was not found");
                    return NotFound();
                }

                var author = await authorRepository.FindById(id);
                var isSuccess = await authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Delete Failed");
                }

                logger.LogWarn($"{location}: id: {id} succesfully deleted");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

    }
}

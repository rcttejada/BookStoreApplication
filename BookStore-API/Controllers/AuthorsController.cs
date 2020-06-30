using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
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

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                logger.LogInfo("Executed Get All Authors");
                var authors = await authorRepository.FindAll();
                var response = mapper.Map<IList<AuthorDTO>>(authors);
                logger.LogInfo("Successfully got all Authors");

                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get Author by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>An Author Record</returns>
        [HttpGet("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int Id)
        {
            try
            {
                logger.LogInfo("Executed Get Author with Id:{Id}");
                var author = await authorRepository.FindById(Id);

                if (author == null)
                {
                    logger.LogWarn($"Author with Id:{Id} was not found");
                    return NotFound();
                }
                var response = mapper.Map<AuthorDTO>(author);
                logger.LogInfo("Successfully got Author with Id:{Id}");

                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                logger.LogInfo($"Author Submission Attempted");
                if(authorDTO == null)
                {
                    logger.LogWarn($"Author Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    logger.LogWarn($"Author Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var author = mapper.Map<Author>(authorDTO);
                var isSuccess = await authorRepository.Create(author);

                if (!isSuccess)
                {
                    return InternalError($"Author creation failed");
                }
                logger.LogInfo("Author Created");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int Id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                logger.LogInfo($"Author Update Attempt - Id:{Id}");
                if(Id < 1 || authorDTO == null || Id != authorDTO.Id)
                {
                    logger.LogWarn($"Author Update failed with bad data");
                    return BadRequest();
                }

                var isExists = await authorRepository.isExists(Id);
                if (!isExists)
                {
                    logger.LogWarn($"Author with id:{Id} was not found");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    logger.LogWarn($"Author Update Data Incomplete");
                    return BadRequest(ModelState);
                }

                var author = mapper.Map<Author>(authorDTO);
                var isSuccess = await authorRepository.Update(author);

                if (!isSuccess)
                {
                    return InternalError($"Update operation failed");
                }
                logger.LogInfo($"Author with Id:{Id} successfully updated");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Removes an author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ///         [HttpPut("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                logger.LogInfo($"Author with id: {id} Delete Attempted");
                if(id < 1)
                {
                    logger.LogInfo($"Author Delete Failed with bad data");
                    return BadRequest();
                }

                var isExists = await authorRepository.isExists(id);
                if (!isExists)
                {
                    logger.LogWarn($"Author with id:{id} was not found");
                    return NotFound();
                }

                var author = await authorRepository.FindById(id);
                var isSuccess = await authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"Author Delete Failed");
                }

                logger.LogWarn($"Author with id: {id} succesfully deleted");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

    }
}

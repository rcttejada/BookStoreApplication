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
using NLog;

namespace BookStore_API.Controllers
{

    /// <summary>
    /// Interacts with the Books table
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository bookRepository;
        private readonly ILoggerService logger;
        private readonly IMapper mapper;

        public BooksController(IBookRepository bookRepository, ILoggerService logger, IMapper mapper)
        {
            this.bookRepository = bookRepository;
            this.logger = logger;
            this.mapper = mapper;
        }


        private ObjectResult InternalError(string message)
        {
            logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }


        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns>List of Books</returns>
        [HttpGet]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Attmpted Call");
                var books = await bookRepository.FindAll();
                var response = mapper.Map<IList<BookDTO>>(books);
                logger.LogInfo($"{location}: Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Gets Book by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A Book record</returns>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrator, Customer")]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Attmpted Call for id: {id}");
                var book = await bookRepository.FindById(id);
                if (book == null)
                {
                    logger.LogWarn($"{location}: Failed to retrieve record with id: {id}");
                    return NotFound();
                }
                var response = mapper.Map<BookDTO>(book);
                logger.LogInfo($"{location}: Successful got record by id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Creates a new Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns>Book Object</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Create Attempted");
                if (bookDTO == null)
                {
                    logger.LogWarn($"{location}: Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    logger.LogWarn($"{location}: Data was incomplete");
                    return BadRequest(ModelState);
                }
                var book = mapper.Map<Book>(bookDTO);
                var isSuccess = await bookRepository.Create(book);

                if (!isSuccess)
                {
                    return InternalError($"{location}: Creation Failed");
                }
                logger.LogInfo($"{location}: Creation was succesful");
                logger.LogInfo($"{location}: {book}");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Returns book by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Update Attempted on record with id:{id}");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    logger.LogWarn($"{location}: Update with bad data - id: {id}");
                    return BadRequest();
                }
                var isExists = await bookRepository.isExists(id);
                if (!isExists)
                {
                    logger.LogWarn($"{location}: Failed to retrieve record with id: {id}");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    logger.LogWarn($"{location}: Data was Incomplete");
                }
                var book = mapper.Map<Book>(bookDTO);
                var isSuccess = await bookRepository.Update(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Update Failed");
                }
                logger.LogWarn($"{location}: Record with id: {id} successfulyy updated");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Remove book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                logger.LogInfo($"{location}: Delete Attempted on record with - id: {id}");
                if (id < 1)
                {
                    logger.LogWarn($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest();
                }

                var isExists = await bookRepository.isExists(id);
                if (!isExists)
                {
                    logger.LogWarn($"{location}: Delete failed to retrieve record with - id: {id}");
                    return NotFound();
                }
                var book = await bookRepository.FindById(id);
                var isSuccess = await bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Delete failed for record with id: {id}");
                }
                logger.LogInfo($"{location}: Record with id: {id} successfully deleted");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
    }
}

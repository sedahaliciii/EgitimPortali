using AutoMapper;
using EgitimPortaliAPI.DTOs;
using EgitimPortaliAPI.Models;
using EgitimPortaliAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EgitimPortaliAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(CategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        // GET: api/Category
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllCategories([FromQuery] bool includeInactive = true) // varsayılan olarak tümünü getir
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesIncludingInactive();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                Console.WriteLine($"GetCategoryById çağrıldı - id: {id}");
                var category = await _categoryRepository.GetCategoryById(id);

                if (category == null)
                {
                    Console.WriteLine($"Kategori bulunamadı - id: {id}");
                    return NotFound($"Category with ID {id} not found");
                }

                Console.WriteLine($"Kategori bulundu - id: {id}");
                return Ok(category);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto categoryDto)
        {
            try
            {
                // Metot çağrılarında parentez kullanın
                Console.WriteLine($"CreateCategory çağrıldı - data: {System.Text.Json.JsonSerializer.Serialize(categoryDto)}");
                var category = _mapper.Map<Category>(categoryDto);
                await _categoryRepository.CreateCategory(category);
                Console.WriteLine($"Kategori oluşturuldu - id: {category.Id}");
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto categoryDto)
        {
            try
            {
                Console.WriteLine($"UpdateCategory çağrıldı - id: {id}, data: {System.Text.Json.JsonSerializer.Serialize(categoryDto)}");

                var existingCategory = await _categoryRepository.GetCategoryById(id);
                if (existingCategory == null)
                {
                    Console.WriteLine($"Kategori bulunamadı - id: {id}");
                    return NotFound($"Category with ID {id} not found");
                }

                _mapper.Map(categoryDto, existingCategory);
                await _categoryRepository.UpdateCategory(existingCategory);

                Console.WriteLine($"Kategori güncellendi - id: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                Console.WriteLine($"DeleteCategory çağrıldı - id: {id}");

                var category = await _categoryRepository.GetCategoryById(id);
                if (category == null)
                {
                    Console.WriteLine($"Kategori bulunamadı - id: {id}");
                    return NotFound($"Category with ID {id} not found");
                }

                // Kategori ID'sini gönderiyoruz, tüm kategori nesnesini değil
                await _categoryRepository.DeleteCategory(id);

                Console.WriteLine($"Kategori silindi - id: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
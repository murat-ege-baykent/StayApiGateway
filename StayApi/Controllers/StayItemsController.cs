using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayApi.Models;

namespace StayApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class StayItemsController : ControllerBase
    {
        private readonly StayContext _context;

        public StayItemsController(StayContext context)
        {
            _context = context;
        }

        // POST: api/StayItems/InsertListing
        [HttpPost("InsertListing")]
        //[Authorize(Roles = "Host")] // Only Hosts can access
        public async Task<ActionResult> InsertListing(
            int maxNoOfPeople,
            string country,
            string city,
            int price)
        {
            if(price <= 0 || maxNoOfPeople <= 0)
            {
                return BadRequest(new { Status = "Error", Message = "Invalid data." });
            }
            StayItem stayItem = new StayItem();
            stayItem.MaxNoOfPeople = maxNoOfPeople;
            stayItem.Country = country;
            stayItem.City = city;
            stayItem.Price = price;

            if (stayItem == null)
            {
                return BadRequest(new { Status = "Error", Message = "Error creating the stayItem." });
            }

            _context.StayItems.Add(stayItem);
            await _context.SaveChangesAsync();

            return Ok(new { Status = "Successful", Message = "Listing added successfully." });
        }
        
        // GET: api/StayItems/QueryListings
        [HttpGet("QueryListings")]
        public async Task<ActionResult<IEnumerable<StayItem>>> QueryListings(
            DateTime dateFrom,
            DateTime dateTo,
            int noOfPeople,
            string country,
            string city,
            int pageNumber = 1,
            int pageSize = 5)
        {
            if(dateFrom > dateTo)
            {
            return BadRequest(new { Status = "Error", Message = "dateFrom must be smaller than dateTo" });
            }
            if(noOfPeople <= 0)
            {
                return BadRequest(new { Status = "Error", Message = "noOfPeople must be greater than 0." });
            }
            var query = _context.StayItems.Where(item => item.Country == country && item.City == city && noOfPeople <= item.MaxNoOfPeople);

            foreach(var stayItem in query)
            {
                int sum = 0;
                var query2 = _context.BookingItems.Where(booking => booking.StayId == stayItem.StayId);

                if(query2 == null)
                {
                    if(noOfPeople <= stayItem.MaxNoOfPeople)
                    {
                        var listings = await query
                        .Select(item => new
                        {
                            item.StayId,
                            item.Country,
                            item.City,
                            item.MaxNoOfPeople,
                            item.Price,
                            item.Rating
                        })
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                        await _context.SaveChangesAsync();
                        return Ok(listings);
                    }else return BadRequest(new { Status = "Error", Message = "Not enough capacity." });
                }

                foreach (var bookingItem in query2)
                {
                    if(bookingItem.AvailableFrom <= dateTo && bookingItem.AvailableTo >= dateFrom)
                    {
                        sum += bookingItem.NoOfPeople;
                    }
                }
                if(sum+noOfPeople <= stayItem.MaxNoOfPeople)
                {
                    var queryUpdated = query.Where(item => (sum+noOfPeople) <= item.MaxNoOfPeople);
                    var listings = await queryUpdated
                    .Select(item => new
                    {
                        item.StayId,
                        item.Country,
                        item.City,
                        item.MaxNoOfPeople,
                        item.Price,
                        item.Rating
                    })
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                    await _context.SaveChangesAsync();
                    return Ok(listings);
                } else return BadRequest(new { Status = "Error", Message = "Not enough capacity." });
            }
            return NotFound();
        }

        // POST: api/StayItems/BookStay
        [HttpPost("BookStay")]
        //[Authorize(Roles = "Guest")] // Only authorized Guests can access
        public async Task<ActionResult> BookStay(
            long stayId,
            DateTime dateFrom,
            DateTime dateTo,
            int noOfPeople,
            List<string> namesOfPeople)
        {
            var stayItem = _context.StayItems.Find(stayId);
            if(stayItem == null)
            {
                return BadRequest(new { Status = "Error", Message = "stayItem not found." });
            }

            if(dateFrom > dateTo)
            {
            return BadRequest(new { Status = "Error", Message = "dateFrom must be smaller than dateTo" });
            }

            var query = _context.BookingItems.Where(booking => booking.StayId == stayId);
            int sum = 0;

            if(query == null)
            {
                if(noOfPeople > stayItem.MaxNoOfPeople)
                {
                    return BadRequest(new { Status = "Error", Message = "Not enough capacity." });
                }
                BookingItem bookingItem = new BookingItem();
                bookingItem.AvailableFrom = dateFrom;
                bookingItem.AvailableTo = dateTo;
                bookingItem.NoOfPeople = noOfPeople;
                bookingItem.NamesOfPeople = namesOfPeople;
                bookingItem.StayId = stayId;

                if (bookingItem == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Invalid data." });
                }

                _context.BookingItems.Add(bookingItem);
                await _context.SaveChangesAsync();

                return Ok(new { Status = "Successful", Message = "Booking added successfully." });
            }

            foreach (var bookingItem in query)
            {
                if(bookingItem.AvailableFrom <= dateTo && bookingItem.AvailableTo >= dateFrom)
                {
                    sum += bookingItem.NoOfPeople;
                }   
            }         
            if(sum+noOfPeople <= stayItem.MaxNoOfPeople)
            {
                BookingItem bookingItem = new BookingItem();
                bookingItem.AvailableFrom = dateFrom;
                bookingItem.AvailableTo = dateTo;
                bookingItem.NoOfPeople = noOfPeople;
                bookingItem.NamesOfPeople = namesOfPeople;
                bookingItem.StayId = stayId;

                if (bookingItem == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Invalid data." });
                }

                    _context.BookingItems.Add(bookingItem);
                await _context.SaveChangesAsync();

                return Ok(new { Status = "Successful", Message = "Booking added successfully." });
            } else return BadRequest(new { Status = "Error", Message = "Not enough capacity." });
        }

        [HttpPatch("ReviewStayOutOfTen")]
        //[Authorize(Roles = "Guest")] // Only authorized Guests can access
        public async Task<ActionResult> ReviewStayOutOfTen(
            long bookingId,
            double rating,
            string comment)
        {
            BookingItem bookingItem = _context.BookingItems.First(item => item.BookingId == bookingId);
            
            if (bookingItem == null)
            {
                return BadRequest(new { Status = "Error", Message = "bookingItem not found" });
            }

            bookingItem.Comment = comment;

            StayItem stayItem = _context.StayItems.First(item => item.StayId == bookingItem.StayId);
            
            if (stayItem == null)
            {
                return BadRequest(new { Status = "Error", Message = "stayItem does not have a bookingItem posted" });
            }

            if(!(0 <= rating && rating <= 10))
            {
                return BadRequest(new { Status = "Error", Message = "Rating must be between 0 and 10" });
            }

            stayItem.Rating += rating;
            stayItem.Rating /= 2; 
            
            await _context.SaveChangesAsync();
            return Ok(new { Status = "Successful", Message = "Review sent successfully." });
        }

        [HttpGet("ReportListings")]
        //[Authorize(Roles = "Admin")] // Only Admins can access
        public async Task<ActionResult<IEnumerable<StayItem>>> ReportListings(
            string country,
            string city,
            int pageNumber = 1,
            int pageSize = 5)
        {
            var query = _context.StayItems.Where(item => item.City == city && item.Country == country).AsQueryable();

            if(query == null)
            {
                return BadRequest(new { Status = "Error", Message = "Invalid stayItem data." });
            }
            
            var listings = await query
                .Select(item => new
                {
                    item.StayId,
                    item.Country,
                    item.City,
                    item.MaxNoOfPeople,
                    item.Price,
                    item.Rating
                })
                .OrderByDescending(item => item.Rating)
                .Skip((pageNumber - 1) * pageSize) 
                .Take(pageSize)
                .ToListAsync();

            return Ok(listings);
        }
    }
}

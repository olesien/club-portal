﻿using club.Data;
using club.Dtos;
using club.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;

namespace club.Controllers
{
    [ApiController]
    [Route("club")]
    public class ClubController : ExtendedController
    {
        // GET: /club
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Club>>> GetUserClubs(
            [FromServices] MyDbContext _context)
        {
            var result = await GetCurrentUser(_context);
            if (result.Result != null) // If it's an error result
                return result.Result;
            if (result.Value == null) return NotFound();
            ApplicationUser user = result.Value;

            return Ok(user.Clubs.Select(club => new ClubDto
            {
                Id = club.Id,
                Name = club.Name
            }).ToList());
        }

        [HttpGet]
        [Authorize]
        [Route("{id}/info")]
        public async Task<ActionResult<IEnumerable<ClubDto>>> GetClubSuggestions(int id,
    [FromServices] MyDbContext _context)
        {
            var result = await GetCurrentUser(_context);
            if (result.Result != null) // If it's an error result
                return result.Result;
            if (result.Value == null) return NotFound();
            ApplicationUser user = result.Value;

            // Fetch the club and its users, suggestions, meetings
            var club = await _context.Club
                .Include(u => u.Users)
                .Include(u => u.Meetings)
                .Include(u => u.MeetingsSuggestions)
                .ThenInclude(u => u.MeetingsSuggestionsUpvotes)
                .Include(u => u.MeetingsSuggestions)
                .ThenInclude(u => u.MeetingsSuggestionsDownvotes)
                .FirstOrDefaultAsync(c => c.Id == id);

            //If there is no club by the given ID, not found.
            if (club == null)
            {
                return NotFound();
            }

            //Order
            club.Meetings = club.Meetings.OrderByDescending(m => m.MeetingTime).ToList();
            club.MeetingsSuggestions = club.MeetingsSuggestions.OrderByDescending(ms => ms.Id).ToList();
            //Try to find the current userId in the list of users. If it can't be found user is not part of this club
            var userRef = club.Users.FirstOrDefault(u => u.Id == user.Id);
            if (userRef == null)
            {
                return Unauthorized();
            }
            var clubDto = new ClubDto
            {
                Id = club.Id,
                Name = club.Name,
                Meetings = club.Meetings.Select(meeting =>
                 new MeetingDTO
                 {
                     Id = meeting.Id,
                     Name = meeting.Name,
                     Description = meeting.Description,
                     MeetingTime = meeting.MeetingTime,
                     Location = meeting.Location
                 }).ToList(),
                MeetingsSuggestions = club.MeetingsSuggestions.Select(suggestion =>
               new MeetingSuggestionDTO
               {
                   Id = suggestion.Id,
                   Title = suggestion.Title,
                   Description = suggestion.Description,
                   CreatedAt = suggestion.CreatedAt,
                   User = new UserDTO
                   {
                       Id = suggestion.User.Id,
                       FirstName = suggestion.User.FirstName,
                       LastName = suggestion.User.LastName,
                       Email = suggestion.User.Email ?? "",
                       UserName = suggestion.User.UserName ?? ""

                   },
                   MeetingsSuggestionsUpvotes = suggestion.MeetingsSuggestionsUpvotes.Select(upvote => new MeetingSuggestionUpvoteDTO
                   {
                       Id = upvote.Id,
                       UserId = upvote.User.Id
                   }).ToList(),
                   MeetingsSuggestionsDownvotes = suggestion.MeetingsSuggestionsDownvotes.Select(upvote => new MeetingSuggestionDownvoteDTO
                   {
                       Id = upvote.Id,
                       UserId = upvote.User.Id
                   }).ToList(),

               }).ToList()
            };

            return Ok(clubDto);
        }

        [HttpPost]
        [Authorize]
        [Route("suggestion/{clubId}")]
        public async Task<ActionResult<String>> AddProposal(AddSuggestion suggestionDTO,
           [FromServices] MyDbContext _context, int clubId)
        {
            var result = await GetCurrentUser(_context);
            if (result.Result != null) // If it's an error result
                return result.Result;
            if (result.Value == null) return NotFound();
            ApplicationUser user = result.Value;

            if (user.Clubs.IsNullOrEmpty())
            {
                return NotFound();
            }
            var club = user.Clubs.Where(club => club.Id == clubId).FirstOrDefault();
            if (club == null) return NotFound();
            var meetingSuggestion = new MeetingsSuggestion();
            meetingSuggestion.Title = suggestionDTO.Title;
            meetingSuggestion.Description = suggestionDTO.Description;
            meetingSuggestion.Club = club;
            meetingSuggestion.User = user;

            _context.MeetingsSuggestion.Add(meetingSuggestion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(AddProposal), user.Id);
        }

        [HttpDelete]
        [Authorize]
        [Route("suggestion/delete/{suggestionid}")]
        public async Task<ActionResult<String>> RemoveProposal(
           [FromServices] MyDbContext _context, int suggestionid)
        {
            var result = await GetCurrentUser(_context);
            if (result.Result != null) // If it's an error result
                return result.Result;
            if (result.Value == null) return NotFound();
            ApplicationUser user = result.Value;

            if (user.Clubs.IsNullOrEmpty())
            {
                return NotFound();
            }
            var meetingSuggestion = _context.MeetingsSuggestion.Where(sugg => sugg.Id == suggestionid && user.Clubs.Select(c => c.Id).Contains(sugg.Club.Id)).FirstOrDefault();
            if (meetingSuggestion == null) return NotFound();

            _context.MeetingsSuggestion.Remove(meetingSuggestion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(RemoveProposal), user.Id);
        }

        [HttpPost]
        [Authorize]
        [Route("react")]
        public async Task<ActionResult<String>> ProposalReact(React reactDTO,
           [FromServices] MyDbContext _context)
        {
            var result = await GetCurrentUser(_context);
            if (result.Result != null) // If it's an error result
                return result.Result;
            if (result.Value == null) return NotFound();
            ApplicationUser user = result.Value;

            if (user.Clubs.IsNullOrEmpty())
            {
                return NotFound();
            }

            //Check if user is in the same club as the suggestion
            var suggestion = await _context.MeetingsSuggestion
                .Include(s => s.Club)
                .Where(sugg => sugg.Id == reactDTO.SuggestionId)
                .Where(sugg => user.Clubs.Select(c => c.Id).Contains(sugg.Club.Id))
                .FirstOrDefaultAsync();

            if (suggestion == null)
            {
                return NotFound();
            }

            //Delete previous reacts on this suggestion by this user
            var upvotes = await _context.MeetingsSuggestionsUpvote
                .Where(react => react.MeetingsSuggestion.Id == suggestion.Id && react.User.Id == user.Id)
                .ToListAsync();
            var downvotes = await _context.MeetingsSuggestionsDownvote
                .Where(react => react.MeetingsSuggestion.Id == suggestion.Id && react.User.Id == user.Id)
                .ToListAsync();

            _context.MeetingsSuggestionsUpvote.RemoveRange(upvotes);
            _context.MeetingsSuggestionsDownvote.RemoveRange(downvotes);
            //Add react
            if (reactDTO.IsUpvote)
            {
                if (upvotes.Count == 0) //We only want to add if it doesn't already exist. If it exists, this is a DELETE operation
                {
                    _context.MeetingsSuggestionsUpvote.Add(new MeetingsSuggestionsUpvote
                    {
                        User = user,
                        MeetingsSuggestion = suggestion
                    });
                }

            }
            else
            {
                if (downvotes.Count == 0) // We only want to add if it doesn't already exist. If it exists, this is a DELETE operation
                {
                    _context.MeetingsSuggestionsDownvote.Add(new MeetingsSuggestionsDownvote
                    {
                        User = user,
                        MeetingsSuggestion = suggestion
                    });
                }
            }
            await _context.SaveChangesAsync();


            //Respond
            return CreatedAtAction(nameof(ProposalReact), user.Id);
        }

        // GET: /club/admin
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        //[Authorize]
        public String AdminOs()
        {
            return "pong";
        }
    }
}
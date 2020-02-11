﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CS321_W5D1_ExerciseLogAPI.ApiModels;
using CS321_W5D1_ExerciseLogAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CS321_W5D1_ExerciseLogAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ActivitiesController : Controller
    {
        private IActivityService _activityService;

        public ActivitiesController(IActivityService activitieservice)
        {
            _activityService = activitieservice;
        }

        private string CurrentUserId
        {
            get
            {
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        // GET api/activities
        [HttpGet]
        public IActionResult Get()
        {
            if (User.IsInRole("Admin"))
            {
                var allActivities = _activityService
                    .GetAll()
                    .ToApiModels();

                return Ok(allActivities);
            }

            var activityModels = _activityService
                .GetAllForUser(CurrentUserId)
                .ToApiModels();

            return Ok(activityModels);
        }

        // get specific activity by id
        // GET api/activities/:id
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var activity = _activityService.Get(id);
            if (activity == null) return NotFound();

            if (activity.UserId != CurrentUserId && !User.IsInRole("Admin"))
            {
                ModelState.AddModelError("UserId", "You can only retrieve your own activities.");
                return BadRequest(ModelState);
            }
            return Ok(activity.ToApiModel());
        }

        // create a new activity
        // POST api/activities
        [HttpPost]
        public IActionResult Post([FromBody] ActivityModel activityModel)
        {
            try
            {
                // add the new activity
                _activityService.Add(activityModel.ToDomainModel());
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("AddActivity", ex.GetBaseException().Message);
                return BadRequest(ModelState);
            }

            return CreatedAtAction("Get", new { Id = activityModel.Id }, activityModel);
        }

        // PUT api/activities/:id
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] ActivityModel updatedActivity)
        {
            var activity = _activityService.Update(updatedActivity.ToDomainModel());
            if (activity == null) return NotFound();

            if(activity.UserId != CurrentUserId && !User.IsInRole("Admin"))
            {
                ModelState.AddModelError("UserId", "You can only update your own activities.");
                return BadRequest(ModelState);
            }
            return Ok(activity.ToApiModel());
        }

        // DELETE api/activities/:id
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var activity = _activityService.Get(id);
            if (activity == null) return NotFound();

            if(activity.UserId != CurrentUserId && !User.IsInRole("Admin"))
            {
                ModelState.AddModelError("UserId", "You can only delete your own activities.");
                return BadRequest(ModelState);
            }

            _activityService.Remove(activity);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public IActionResult Delete()
        {
            return Ok("All activities deleted!");
        }
    }
}
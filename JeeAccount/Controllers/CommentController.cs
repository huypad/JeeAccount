using DPSinfra.UploadFile;
using JeeAccount.Classes;
using JeeAccount.Services.CommentService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            this._commentService = commentService;
        }

        [HttpGet("getByComponentName/{componentName}")]
        public async Task<IActionResult> GetByComponentName(string componentName)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(NotFound(MessageReturnHelper.CustomDataKhongTonTai()));
                }
                var username = Ulities.GetUsernameByHeader(HttpContext.Request.Headers);

                var topicObjectID = await _commentService.GetTopicObjectID(componentName);
                if (!string.IsNullOrEmpty(topicObjectID))
                {
                    return Ok(topicObjectID);
                }
                else
                {
                    var responseMessage = await _commentService.CreateTopicObjectID(username, access_token);
                    if (!responseMessage.IsSuccessStatusCode) return BadRequest(MessageReturnHelper.Custom($"Error {responseMessage.ReasonPhrase}"));
                    var newTopic = Newtonsoft.Json.JsonConvert.DeserializeObject<ConvertTopic>(responseMessage.Content.ReadAsStringAsync().Result);
                    _commentService.SaveTopicID(componentName, newTopic.Id);
                    return Ok(newTopic.Id);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("postComment")]
        public async Task<IActionResult> PostComment([FromBody] PostCommentModel postComment)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(NotFound(MessageReturnHelper.CustomDataKhongTonTai()));
                }
                var username = Ulities.GetUsernameByHeader(HttpContext.Request.Headers);
                postComment.Username = username;
                if (postComment.Attachs != null)
                {
                    postComment.Attachs = _commentService.UpdateAllFileToMinio(postComment.Attachs, username);
                }
                await _commentService.PostCommentKafka(postComment);
                return StatusCode(201, postComment);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("postReactionComment")]
        public async Task<IActionResult> PostReactionComment([FromBody] ReactionCommentModel reactionCommentModel)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(NotFound(MessageReturnHelper.CustomDataKhongTonTai()));
                }
                reactionCommentModel.Username = Ulities.GetUsernameByHeader(HttpContext.Request.Headers);

                await _commentService.PostReactionCommentKafka(reactionCommentModel);
                return StatusCode(200, reactionCommentModel);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }
    }

    public class KafkaCommentModel
    {
        public bool IsAddNew { get; set; } = false;
        public bool IsUpdate { get; set; } = false;
        public bool IsDelete { get; set; } = false;
        public bool IsComment { get; set; } = false;
        public bool IsReaction { get; set; } = false;
        public PostCommentModel PostComment { get; set; } = null;
        public ReactionCommentModel ReactionComment { get; set; } = null;
    }

    public class ConvertTopic
    {
        public string Id { get; set; }
    }

    public class PostCommentModel
    {
        public string TopicCommentID { get; set; }
        public string CommentID { get; set; }
        public string ReplyCommentID { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public Attach Attachs { get; set; }
    }

    public class ReactionCommentModel
    {
        public string TopicCommentID { get; set; }
        public string CommentID { get; set; }
        public string ReplyCommentID { get; set; }
        public string Username { get; set; }
        public string UserOldReaction { get; set; }
        public string UserReaction { get; set; }
    }

    public class Attach
    {
        public List<string> Images { get; set; }
        public List<string> Files { get; set; }
        public List<string> Videos { get; set; }
    }
}
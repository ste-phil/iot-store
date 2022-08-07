using HomeApp.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace HomeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IotController : ControllerBase
    {
        private readonly AppDbContext ctx;

        public IotController(AppDbContext ctx)
        {
            this.ctx = ctx;
        }

        [HttpGet]
        public string ListBuckets()
        {
            StringBuilder sb = new StringBuilder();


            foreach (var bucket in ctx.Buckets)
            {
                sb.Append(bucket.Name);
                sb.AppendLine();
            }

            return ctx.Buckets.Count() == 0 ? "No buckets found" : sb.ToString();
        }

        [HttpDelete("{bucketName}")]
        public ActionResult DeleteBucket([FromRoute] string bucketName)
        {
            var bucket = ctx.Buckets.FirstOrDefault(x => x.Name == bucketName);
            if (bucket == null) return NotFound();
            
            ctx.Buckets.Remove(bucket);
            ctx.SaveChanges();
            return Ok();
        }

        [HttpPost("{bucketName}")]
        public async Task<ActionResult> Publish([FromRoute] string bucketName)
        {
            bool bucketCreated = false;

            var bucket = ctx.Buckets.FirstOrDefault(x => x.Name == bucketName);
            if (bucket == null) {
                bucket = new Bucket { Name = bucketName};
                ctx.Buckets.Add(bucket);
                bucketCreated = true;
            }

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {  
                string content = await reader.ReadToEndAsync();
                
                if (content != null && content != "") {
                    ctx.Entries.Add(new Entry {
                        Timestamp = DateTime.UtcNow,
                        Value = content,
                        BucketId = bucketName
                    });
                    await ctx.SaveChangesAsync();
                    return Ok($"{(bucketCreated ? "Bucket created - " : "")}Data added");;
                }

                await ctx.SaveChangesAsync();
                return Ok($"{(bucketCreated ? "Bucket created - " : "")}No body supplied");
            }
        }

        [HttpGet("{bucketName}")]
        public async Task<ActionResult> Download(string bucketName)
        {
            var bucket = ctx.Buckets.FirstOrDefault(x => x.Name == bucketName);
            if (bucket == null || bucket.Entries.Count == 0) return NotFound();

            string fileName = $"{bucket.Name}.csv";
            byte[] fileBytes = ToCSVBytes(bucket);

            return File(fileBytes, "text/csv", fileName);
        }


        private byte[] ToCSVBytes(Bucket bucket) 
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in bucket.Entries)
            {
                sb.Append(item.Timestamp);
                sb.Append(",");
                sb.Append(item.Value);
                sb.AppendLine();
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
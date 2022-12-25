using System.Threading.Channels;
using Eocron.Sharding.TestWebApp.Shards;
using Microsoft.AspNetCore.Mvc;

namespace Eocron.Sharding.TestWebApp.Controllers
{
    [ApiController]
    [Route("api/v1/shards")]
    public class ShardControllerV1 : ControllerBase
    {
        private readonly IShardProvider<string, string, string> _shardProvider;

        public ShardControllerV1(IShardProvider<string, string, string> shardProvider)
        {
            _shardProvider = shardProvider;
        }

        [HttpGet("search")]
        public IActionResult SearchShards(bool? ready)
        {
            return Ok(SearchShardIds(ready));
        }

        [HttpGet("{id}/is_ready")]
        public IActionResult IsReady([FromRoute(Name = "id")] string shardId)
        {
            var shard = _shardProvider.FindShardById(shardId);
            if (shard == null)
                return NotFound();
            return Ok(shard.IsReady());
        }

        [HttpPost("{id}/fetch_errors")]
        public IActionResult FetchErrors([FromRoute(Name = "id")] string shardId)
        {
            var shard = _shardProvider.FindShardById(shardId);
            if (shard == null)
                return NotFound();
            var result = FetchLatest(shard.Errors, 100);
            return Ok(result);
        }

        [HttpPost("{id}/fetch_outputs")]
        public IActionResult FetchOutput([FromRoute(Name = "id")] string shardId)
        {
            var shard = _shardProvider.FindShardById(shardId);
            if (shard == null)
                return NotFound();
            var result = FetchLatest(shard.Outputs, 100);
            return Ok(result);
        }

        [HttpPost("{id}/restart")]
        public async Task<IActionResult> Restart([FromRoute(Name = "id")] string shardId, CancellationToken ct)
        {
            var shard = _shardProvider.FindShardById(shardId);
            if (shard == null)
                return NotFound();
            await shard.CancelAsync(ct).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPost("{id}/publish")]
        public async Task<IActionResult> PublishAsync([FromRoute(Name = "id")]string shardId, [FromBody]string[] messages, CancellationToken ct)
        {
            var shard = _shardProvider.FindShardById(shardId);
            if (shard == null)
                return NotFound();
            await shard.PublishAsync(messages, ct).ConfigureAwait(false);
            return NoContent();
        }

        private List<string> SearchShardIds(bool? isReady)
        {
            return _shardProvider
                .GetAllShards()
                .Where(x => isReady == null || (isReady.Value ? x.IsReady() : !x.IsReady()))
                .Select(x => x.Id)
                .ToList();
        }

        private static IList<T> FetchLatest<T>(ChannelReader<T> channel, int maxSize)
        {
            var result = new List<T>();
            while (result.Count != maxSize && channel.TryRead(out var item))
            {
                result.Add(item);
            }
            return result;
        }
    }
}
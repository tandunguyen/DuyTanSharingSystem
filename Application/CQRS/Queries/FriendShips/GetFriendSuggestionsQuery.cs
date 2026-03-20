using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.FriendShips
{
    public class GetFriendSuggestionsQuery : IRequest<ResponseModel<List<FriendSuggestionDto>>>
    {
        public int Limit { get; set; } = 10;
        public int Offset { get; set; } = 0;

        public GetFriendSuggestionsQuery() { }
        public GetFriendSuggestionsQuery(int limit = 10, int offset = 0)
        {
            Limit = limit;
            Offset = offset;
        }
    }
}

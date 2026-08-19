using Regira.Entities.QueryBuilders.Abstractions;

namespace RoomPlanner.Api.Entities.MeetingRooms;

public class MeetingRoomQueryBuilder : FilteredQueryBuilderBase<MeetingRoom, int, MeetingRoomSearchObject>
{
    public override IQueryable<MeetingRoom> Build(IQueryable<MeetingRoom> query, MeetingRoomSearchObject? so)
    {
        if (so == null)
        {
            return query;
        }

        if (so.FloorId?.Any() == true)
        {
            query = query.Where(x => so.FloorId.Contains(x.FloorId));
        }
        if (so.BuildingId?.Any() == true)
        {
            query = query.Where(x => so.BuildingId.Contains(x.Floor!.BuildingId));
        }
        if (so.MinCapacity.HasValue)
        {
            query = query.Where(x => x.Capacity >= so.MinCapacity.Value);
        }
        if (so.Equipment.HasValue)
        {
            query = query.Where(x => (x.Equipment & so.Equipment.Value) == so.Equipment.Value);
        }
        if (so.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == so.IsActive.Value);
        }
        if (so.RequiresApproval.HasValue)
        {
            query = query.Where(x => x.RequiresApproval == so.RequiresApproval.Value);
        }

        return query;
    }
}

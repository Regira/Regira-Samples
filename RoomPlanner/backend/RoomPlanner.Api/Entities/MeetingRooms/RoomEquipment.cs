namespace RoomPlanner.Api.Entities.MeetingRooms;

[Flags]
public enum RoomEquipment
{
    None = 0,
    Projector = 1 << 0,
    Whiteboard = 1 << 1,
    VideoConferencing = 1 << 2,
    ConferencePhone = 1 << 3,
    Monitor = 1 << 4,
    Catering = 1 << 5
}

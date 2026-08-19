import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"
import { ReservationAttendee } from "../reservation-attendees/Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Owned child collections use the `_deleted` mark (never splice): removed rows are filtered out here so
    // the server deletes them by omission (Rooms is a pure m2m join, no lift needed; Attendees rows are lifted
    // to real instances in toEntity below).
    protected override prepareItem(item: Entity): Entity {
        item.rooms = item.rooms?.filter((x) => !x._deleted)
        item.attendees = item.attendees?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (typeof (e as any).startTime === "string") e.startTime = new Date((e as any).startTime)
        if (typeof (e as any).endTime === "string") e.endTime = new Date((e as any).endTime)
        if (e.attendees?.some((row) => !(row instanceof ReservationAttendee))) e.attendees = e.attendees.map((row) => ReservationAttendee.create(row))
        return e
    }
}

export default EntityService

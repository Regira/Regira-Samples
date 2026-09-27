import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"
import ReservationAttendee from "../reservation-attendees/Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // null = untouched, [] = delete every row: no `|| []` here
    protected override prepareItem(item: Entity): Entity {
        item.rooms = item.rooms?.filter((x) => !x._deleted)
        item.attendees = item.attendees?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    // idempotent: runs inside computeds — every conversion is guarded
    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (typeof (e as any).start === "string") e.start = new Date((e as any).start)
        if (typeof (e as any).end === "string") e.end = new Date((e as any).end)
        if (typeof (e as any).cancelledOn === "string") e.cancelledOn = new Date((e as any).cancelledOn)
        if (e.attendees?.some((row) => !(row instanceof ReservationAttendee))) e.attendees = e.attendees.map((row) => ReservationAttendee.create(row))
        return e
    }

    // --- workflow endpoints (ReservationWorkflowController) — each answers { item } like GET /{id} ---
    approveRoom(id: number, roomId: number, note?: string): Promise<Entity> {
        return this.postAction(`${id}/rooms/${roomId}/approve`, note)
    }
    rejectRoom(id: number, roomId: number, note?: string): Promise<Entity> {
        return this.postAction(`${id}/rooms/${roomId}/reject`, note)
    }
    cancel(id: number, reason?: string): Promise<Entity> {
        return this.postAction(`${id}/cancel`, reason)
    }
    private async postAction(path: string, note?: string): Promise<Entity> {
        const { data } = await this.axios.post(`${this.config.api}/${path}`, { note })
        return this.processItem(data.item)!
    }
}

export default EntityService

import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Session from "../sessions/Entity"
import Entity from "./Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Owned collections use the `_deleted` mark: drop marked rows at every nesting level so Related()
    // deletes them by omission. No `|| []` — null = untouched, [] = delete every row.
    protected override prepareItem(item: Entity): Entity {
        item.sessions = item.sessions
            ?.filter((x) => !x._deleted)
            .map((s) => Object.assign(s, { speakers: s.speakers?.filter((sp) => !sp._deleted) }))
        return super.prepareItem(item)
    }

    // idempotent: returns an existing instance untouched, lifts the owned sessions only while they are plain JSON
    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (e.sessions?.some((row) => !(row instanceof Session))) e.sessions = e.sessions.map((row) => Session.create(row))
        return e
    }
}

export default EntityService

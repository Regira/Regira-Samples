import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"
import RoomEquipment from "../room-equipments/Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // null = untouched, [] = delete every row: no `|| []` here
    protected override prepareItem(item: Entity): Entity {
        item.equipment = item.equipment?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    // idempotent: runs inside computeds
    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (e.equipment?.some((row) => !(row instanceof RoomEquipment))) e.equipment = e.equipment.map((row) => RoomEquipment.create(row))
        return e
    }
}

export default EntityService

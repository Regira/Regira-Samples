import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity, { type TicketComment } from "./Entity"
import { insertWithAttachments, updateWithAttachments } from "../../entity-attachments/data/functions"

const toDate = (value: unknown) => (typeof value === "string" ? new Date(value) : (value as Date | undefined))

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Files are staged in memory until the owner has an id, so both write paths flush them.
    override async insert(item: Entity): Promise<Entity | undefined> {
        return await insertWithAttachments(this.config.api, item, () => super.insert(item), (saved) => super.update(saved))
    }
    override async update(item: Entity): Promise<Entity | undefined> {
        return await updateWithAttachments(this.config.api, item, () => super.update(item))
    }

    // Owned collections: rows marked _deleted are dropped so Related() deletes them by omission.
    // Comments may ride along but are ignored: the API's input DTO has no Comments (addComment is their writer).
    protected override prepareItem(item: Entity): Entity {
        item.categories = item.categories?.filter((x) => !x._deleted)
        item.attachments = item.attachments?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    /** POST tickets/{id}/comments — the conversation's only write path; the author is the signed-in user. */
    async addComment(ticketId: number, body: string, isInternal: boolean): Promise<TicketComment> {
        const { data } = await this.axios.post(`${this.config.api}/${ticketId}/comments`, { body, isInternal })
        const comment = data.item as TicketComment
        comment.created = toDate(comment.created)
        return comment
    }

    // Idempotent: every conversion is guarded (it runs inside computeds).
    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (typeof (e as any).dueDate === "string") e.dueDate = toDate((e as any).dueDate)
        if (typeof (e as any).closedAt === "string") e.closedAt = toDate((e as any).closedAt)
        if (typeof (e as any).firstResponseAt === "string") e.firstResponseAt = toDate((e as any).firstResponseAt)
        if (e.comments?.some((c) => typeof c.created === "string")) {
            e.comments.forEach((c) => (c.created = toDate(c.created)))
        }
        return e
    }
}

export default EntityService

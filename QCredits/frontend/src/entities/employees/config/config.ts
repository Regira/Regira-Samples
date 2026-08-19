import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/employees"

const config: IConfig = {
    id: Entity.name,
    key: "Employee",
    isComplex: true,

    routePrefix: "employees",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "employees",
    detailsTitle: "employee",
    description: "employee.description",
    icon: "bi bi-people",

    defaultPageSize: 15,

    api,
    searchUrl: api + "/search",
    saveUrl: api
}

export default config

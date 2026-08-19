import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/employee-carry-overs"

const config: IConfig = {
    id: Entity.name,
    key: "EmployeeCarryOver",
    isComplex: true, // has a relation (Employee) -> page

    routePrefix: "employee-carry-overs",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "employeeCarryOvers",
    detailsTitle: "employeeCarryOver",
    description: "employeeCarryOver.description",
    icon: "bi bi-arrow-repeat",

    defaultPageSize: 15,

    api,
    searchUrl: api + "/search",
    saveUrl: api
}

export default config

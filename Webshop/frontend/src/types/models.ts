export interface CategoryCoreDto {
  id: number
  title: string
  slug: string | null
  imageUrl: string | null
}

export interface CategoryDto extends CategoryCoreDto {
  description: string | null
  displayOrder: number
  isFeatured: boolean
  productCount: number | null
  created: string
  lastModified: string | null
}

export interface ProductCoreDto {
  id: number
  title: string
  slug: string | null
  imageUrl: string | null
  price: number
  compareAtPrice: number | null
  stock: number
}

export interface ProductDto extends ProductCoreDto {
  description: string | null
  code: string | null
  brand: string | null
  rating: number
  reviewCount: number
  isFeatured: boolean
  categoryId: number
  category: CategoryCoreDto | null
  created: string
  lastModified: string | null
}

export const OrderStatus = {
  Pending: "Pending",
  Processing: "Processing",
  Shipped: "Shipped",
  Delivered: "Delivered",
  Cancelled: "Cancelled",
} as const
export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus]

export interface OrderLineDto {
  id: number
  orderId: number
  productId: number
  product: ProductCoreDto | null
  quantity: number
  unitPrice: number
  subTotal: number
  sortOrder: number
}

export interface OrderDto {
  id: number
  code: string | null
  status: OrderStatus
  customerName: string
  customerEmail: string
  customerPhone: string | null
  shippingAddress: string
  shippingCity: string
  shippingPostalCode: string
  shippingCountry: string
  total: number
  created: string
  lastModified: string | null
  orderLines: OrderLineDto[] | null
}

export interface OrderLineInputDto {
  productId: number
  quantity: number
}

export interface OrderInputDto {
  customerName: string
  customerEmail: string
  customerPhone?: string | null
  shippingAddress: string
  shippingCity: string
  shippingPostalCode: string
  shippingCountry: string
  orderLines: OrderLineInputDto[]
}

export interface SearchResult<T> {
  items: T[]
  count: number
}

export interface ItemResult<T> {
  item: T
}

export interface FieldErrors {
  [field: string]: string
}

/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface UserResponse {
  id: string;
  username: string;
  role: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: UserResponse;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface CategoryResponse {
  id: string;
  name: string;
}

export interface CategoryCreateRequest {
  name: string;
}

export interface CategoryUpdateRequest {
  id: string;
  name: string;
}

export interface OrderResponse {
  id: string;
  buyerId: string;
  vendorId: string;
  productId: string;
  /** @format int32 */
  quantity: number;
  /** @format decimal */
  unitPriceDkk: number;
  discountApplied: boolean;
  /** @format decimal */
  totalPriceDkk: number;
  /** @format date-time */
  createdAtUtc: string;
  vendorWasRaided: boolean;
}

export interface BuyRequest {
  productId: string;
  /** @format int32 */
  quantity: number;
}

export interface ProductResponse {
  id: string;
  name: string;
  description: string;
  /** @format decimal */
  priceDkk: number;
  /** @format int32 */
  stockCount: number;
  categoryId: string;
  vendorId: string;
  /** @format date-time */
  createdAtUtc: string;
}

export interface ProductCreateRequest {
  name: string;
  description: string;
  /** @format decimal */
  priceDkk: number;
  /** @format int32 */
  stockCount: number;
  categoryId: string;
}

export interface ProductUpdateRequest {
  id: string;
  name: string;
  description: string;
  /** @format decimal */
  priceDkk: number;
  /** @format int32 */
  stockCount: number;
  categoryId: string;
}

export interface StockAdjustmentRequest {
  productId: string;
  /** @format int32 */
  changeBy: number;
}

export interface FeaturedVendorResponse {
  vendorId: string;
  username: string;
  /** @format int32 */
  orderCount: number;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5167";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title My Title
 * @version 1.0.0
 * @baseUrl http://localhost:5167
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  auth = {
    /**
     * No description
     *
     * @tags Auth
     * @name AuthRegister
     * @request POST:/Auth/Register
     */
    authRegister: (data: RegisterRequest, params: RequestParams = {}) =>
      this.request<UserResponse, any>({
        path: `/Auth/Register`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogin
     * @request POST:/Auth/Login
     */
    authLogin: (data: LoginRequest, params: RequestParams = {}) =>
      this.request<LoginResponse, any>({
        path: `/Auth/Login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  categories = {
    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesGetAll
     * @request GET:/Categories/GetAll
     */
    categoriesGetAll: (params: RequestParams = {}) =>
      this.request<CategoryResponse[], any>({
        path: `/Categories/GetAll`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesCreate
     * @request POST:/Categories/Create
     * @secure
     */
    categoriesCreate: (
      data: CategoryCreateRequest,
      params: RequestParams = {},
    ) =>
      this.request<CategoryResponse, any>({
        path: `/Categories/Create`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesUpdate
     * @request PATCH:/Categories/Update
     * @secure
     */
    categoriesUpdate: (
      data: CategoryUpdateRequest,
      params: RequestParams = {},
    ) =>
      this.request<CategoryResponse, any>({
        path: `/Categories/Update`,
        method: "PATCH",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesDelete
     * @request DELETE:/Categories/Delete
     * @secure
     */
    categoriesDelete: (
      query?: {
        id?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Categories/Delete`,
        method: "DELETE",
        query: query,
        secure: true,
        ...params,
      }),
  };
  orders = {
    /**
     * No description
     *
     * @tags Orders
     * @name OrdersBuy
     * @request POST:/Orders/Buy
     * @secure
     */
    ordersBuy: (data: BuyRequest, params: RequestParams = {}) =>
      this.request<OrderResponse, any>({
        path: `/Orders/Buy`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Orders
     * @name OrdersGetMine
     * @request GET:/Orders/GetMine
     * @secure
     */
    ordersGetMine: (params: RequestParams = {}) =>
      this.request<OrderResponse[], any>({
        path: `/Orders/GetMine`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
  };
  products = {
    /**
     * No description
     *
     * @tags Products
     * @name ProductsGetAll
     * @request GET:/Products/GetAll
     */
    productsGetAll: (params: RequestParams = {}) =>
      this.request<ProductResponse[], any>({
        path: `/Products/GetAll`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Products
     * @name ProductsCreate
     * @request POST:/Products/Create
     * @secure
     */
    productsCreate: (data: ProductCreateRequest, params: RequestParams = {}) =>
      this.request<ProductResponse, any>({
        path: `/Products/Create`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Products
     * @name ProductsUpdate
     * @request PATCH:/Products/Update
     * @secure
     */
    productsUpdate: (data: ProductUpdateRequest, params: RequestParams = {}) =>
      this.request<ProductResponse, any>({
        path: `/Products/Update`,
        method: "PATCH",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Products
     * @name ProductsDelete
     * @request DELETE:/Products/Delete
     * @secure
     */
    productsDelete: (
      query?: {
        id?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Products/Delete`,
        method: "DELETE",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Products
     * @name ProductsGetByVendor
     * @request GET:/Products/GetByVendor
     */
    productsGetByVendor: (
      query?: {
        vendorId?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<ProductResponse[], any>({
        path: `/Products/GetByVendor`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Products
     * @name ProductsAdjustStock
     * @request PATCH:/Products/AdjustStock
     * @secure
     */
    productsAdjustStock: (
      data: StockAdjustmentRequest,
      params: RequestParams = {},
    ) =>
      this.request<ProductResponse, any>({
        path: `/Products/AdjustStock`,
        method: "PATCH",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  vendors = {
    /**
     * No description
     *
     * @tags Vendors
     * @name VendorsGetFeatured
     * @request GET:/Vendors/GetFeatured
     */
    vendorsGetFeatured: (params: RequestParams = {}) =>
      this.request<FeaturedVendorResponse[], any>({
        path: `/Vendors/GetFeatured`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
}

# http-mock

Schema of the configuration request:

Configuration:
  └ Map[]
      ┝ url
      ┝ method
      ┝ status
      ┝ delay
      ┝ description
      └ payload

Example of the configuration request:

info: Checkout requests
map:
  - url: /probe
    description: successful probe action
    method: get
    status: 200
    delay: 200

  - url: /probe
    description: failer probe action
    method: get
    status: 502
    delay: 2000

  - url: /probe/detailed
    method: post
    status: 200
    payload: '{"success":"true"}'
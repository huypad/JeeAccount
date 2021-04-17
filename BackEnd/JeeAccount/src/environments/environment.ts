// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  appVersion: 'v717demo1',
  USERDATA_KEY: 'authf649fc9a5f55',

  isMockEnabled: true,
  apiUrl: 'api',

  ApiRoot: 'https://localhost:5001/api',
  ApiRootsLanding: 'https://localhost:44327/api',

  ApiIdentity: 'https://identityserver.jee.vn',
  redirectUrl: 'https://portal.jee.vn/?redirectUrl=',
  sso: 'sso_token',
  //API JeeWork
  ApiJeeWork: 'https://localhost:44366/', // https://localhost:44366/;https://api-proxy.vts-demo.com/
  //Api JeeRequest
  ApiJeeRequest: 'https://jeerequest-api.jee.vn',

  //API JeeAdmin
  ApiJeeAdmin: 'https://jeeadmin-api.jee.vn', //'https://localhost:44360'
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.

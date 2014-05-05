﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using RestSharp;
using Newtonsoft.Json;
using System.ComponentModel;

namespace RippleRest
{
    /// <summary>
    /// Account helper for RippleRest
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Account's Address (rXXXXXX...)
        /// </summary>
        public String Address { get; set; }

        internal SecureString secret { get; set; }

        /// <summary>
        /// Account's secret. Stored in a <see cref="System.Security.SecureString"/>.
        /// </summary>
        public String Secret {
            set
            {
                if (value == null)
                {
                    this.secret = null;
                    return;
                }

                this.secret = new SecureString();
                foreach(char c in value)
                {
                    this.secret.AppendChar(c);
                }
                this.secret.MakeReadOnly();
            }

            internal get
            {
                if (secret == null)
                    return null;

                IntPtr unmanagedString = IntPtr.Zero;
                try
                {
                    unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secret);
                    return Marshal.PtrToStringUni(unmanagedString);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
            }
        }

        /// <summary>
        /// Create a new instance of Account.
        /// </summary>
        /// <param name="address">Account's Address (rXXXXXX...)</param>
        public Account(string address) : this(address, null) { }

        /// <summary>
        /// Create a new instance of Account.
        /// </summary>
        /// <param name="address">Account's Address (rXXXXXX...)</param>
        /// <param name="secret">Account's secret</param>
        public Account(string address, string secret)
        {
            this.Address = address;
            this.Secret = secret;
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class GetBalancesResponse : RestResponseObject
        {
            [JsonProperty("balances")]
            public List<Balance> Balances { set; get; }
        }

        /// <summary>
        /// Get an account's existing balances.
        /// This includes XRP balance (which does not include a counterparty) and trustline balances.
        /// </summary>
        /// <returns>A list of Balance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Balance> GetBalances()
        {
            return GetBalances(RippleRestClient.GetDefaultInstanceOrThrow());
        }

        /// <summary>
        /// Get an account's existing balances.
        /// This includes XRP balance (which does not include a counterparty) and trustline balances.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>A list of Balance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Balance> GetBalances(RippleRestClient client)
        {
            var result = client.RestClient.Execute<GetBalancesResponse>(client.CreateGetRequest("v1/accounts/{0}/balances", Address));
            client.HandleRestResponseErrors(result);

            return result.Data.Balances;
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class GetTrustlinesResponse : RestResponseObject
        {
            [JsonProperty("trustlines")]
            public List<Trustline> Trustlines { set; get; }
        }

        /// <summary>
        /// Returns Trustlines for this account.
        /// </summary>
        /// <returns>A list of Trustline</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Trustline> GetTrustlines()
        {
            return GetTrustlines(RippleRestClient.GetDefaultInstanceOrThrow());
        }

        /// <summary>
        /// Returns Trustlines for this account.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>A list of Trustline</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Trustline> GetTrustlines(RippleRestClient client)
        {
            var result = client.RestClient.Execute<GetTrustlinesResponse>(client.CreateGetRequest("v1/accounts/{0}/trustlines", Address));
            client.HandleRestResponseErrors(result);

            return result.Data.Trustlines;
        }

        /// <summary>
        /// Response of Account.AddTrustline
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        public class AddTrustlineResponse : RestResponseObject
        {
            /// <summary>
            /// Trustline object
            /// </summary>
            [JsonProperty("trustline")]
            public Trustline Trustline { set; get; }

            /// <summary>
            /// Hash
            /// </summary>
            [JsonProperty("hash")]
            public String Hash { set; get; }

            /// <summary>
            /// Ledger
            /// </summary>
            [JsonProperty("ledger")]
            public String Ledger { set; get; }
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class AddTrustlineRequest : RestRequestObject
        {
            [JsonProperty("trustline")]
            public Trustline Trustline { set; get; }

            [JsonProperty("allow_rippling")]
            public bool AllowRippling { set; get; }
        }

        /// <summary>
        /// Add trustline for this account.
        /// </summary>
        /// <param name="trustline">A trustline object.</param>
        /// <returns>An instance of AddTrustlineResponse</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AddTrustlineResponse AddTrustline(Trustline trustline)
        {
            return AddTrustline(RippleRestClient.GetDefaultInstanceOrThrow(), trustline);
        }

        /// <summary>
        /// Add trustline for this account.
        /// </summary>
        /// <param name="allowRippling">Defaults to true. See [here](https://ripple.com/wiki/No_Ripple) for details</param>
        /// <param name="trustline">A trustline object.</param>
        /// <returns>An instance of AddTrustlineResponse</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AddTrustlineResponse AddTrustline(Trustline trustline, bool allowRippling)
        {
            return AddTrustline(RippleRestClient.GetDefaultInstanceOrThrow(), trustline, allowRippling);
        }

        /// <summary>
        /// Add trustline for this account.
        /// </summary>
        /// <param name="trustline">A trustline object.</param>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>An instance of AddTrustlineResponse</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AddTrustlineResponse AddTrustline(RippleRestClient client, Trustline trustline)
        {
            return AddTrustline(client, trustline, true);
        }

        /// <summary>
        /// Add trustline for this account.
        /// </summary>
        /// <param name="allowRippling">Defaults to true. See [here](https://ripple.com/wiki/No_Ripple) for details</param>
        /// <param name="trustline">A trustline object.</param>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>An instance of AddTrustlineResponse</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AddTrustlineResponse AddTrustline(RippleRestClient client, Trustline trustline, bool allowRippling)
        {
            var request = new AddTrustlineRequest();
            request.Secret = this.Secret;
            request.Trustline = trustline;
            request.AllowRippling = allowRippling;

            var result = client.RestClient.Execute<AddTrustlineResponse>(client.CreatePostRequest(request, "v1/accounts/{0}/trustlines", Address));
            client.HandleRestResponseErrors(result);

            return result.Data;
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class GetSettingsResponse : RestResponseObject
        {
            [JsonProperty("settings")]
            public AccountSettings Settings { set; get; }
        }

        /// <summary>
        /// Returns AccountSettings for this account.
        /// </summary>
        /// <returns>AccountSettings instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AccountSettings GetSettings()
        {
            return GetSettings(RippleRestClient.GetDefaultInstanceOrThrow());
        }

        /// <summary>
        /// Returns AccountSettings for this account.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>AccountSettings instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AccountSettings GetSettings(RippleRestClient client)
        {
            var result = client.RestClient.Execute<GetSettingsResponse>(client.CreateGetRequest("v1/accounts/{0}/settings", Address));
            client.HandleRestResponseErrors(result);

            result.Data.Settings.Account = this.Address;
            return result.Data.Settings;
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class SetSettingsRequest : RestRequestObject
        {
            [JsonProperty("settings")]
            public AccountSettings Settings { set; get; }
        }

        /// <summary>
        /// Sets AccountSettings for this account.
        /// </summary>
        /// <param name="value">A AccountSettings instance.</param>
        /// <returns>AccountSettings instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AccountSettings SetSettings(AccountSettings value)
        {
            return SetSettings(RippleRestClient.GetDefaultInstanceOrThrow(), value);
        }

        /// <summary>
        /// Sets AccountSettings for this account.
        /// </summary>
        /// <param name="value">A AccountSettings instance.</param>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>AccountSettings instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public AccountSettings SetSettings(RippleRestClient client, AccountSettings value)
        {
            var data = new SetSettingsRequest
            {
                Settings = value,
                Secret = this.Secret
            };

            var result = client.RestClient.Execute<GetSettingsResponse>(client.CreatePostRequest(data, "v1/accounts/{0}/settings", Address));
            client.HandleRestResponseErrors(result);

            result.Data.Settings.Account = this.Address;
            return result.Data.Settings;
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class GetNotificationResponse : RestResponseObject
        {
            [JsonProperty("notification")]
            public Notification Notification { set; get; }
        }

        /// <summary>
        /// Get notifications.
        /// 
        /// Clients using notifications to monitor their account activity should pay particular attention to the `state` and `result` fields. The `state` field will either be `validated` or `failed` and represents the finalized status of that transaction. The `result` field will be `tesSUCCESS` if the `state` was validated. If the transaction failed, `result` will contain the `rippled` or `ripple-lib` error code.
        /// 
        /// Notifications have `next_notification_url` and `previous_notification_url`'s. Account notifications can be polled by continuously following the `next_notification_url`, and handling the resultant notifications, until the `next_notification_url` is an empty string. This means that there are no new notifications but, as soon as there are, querying the same URL that produced this notification in the first place will return the same notification but with the `next_notification_url` set.
        /// 
        /// </summary>
        /// <param name="hash">Notification hash</param>
        /// <returns>Notification instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public Notification GetNotification(string hash)
        {
            return GetNotification(hash, RippleRestClient.GetDefaultInstanceOrThrow());
        }

        /// <summary>
        /// Get notifications.
        /// 
        /// Clients using notifications to monitor their account activity should pay particular attention to the `state` and `result` fields. The `state` field will either be `validated` or `failed` and represents the finalized status of that transaction. The `result` field will be `tesSUCCESS` if the `state` was validated. If the transaction failed, `result` will contain the `rippled` or `ripple-lib` error code.
        /// 
        /// Notifications have `next_notification_url` and `previous_notification_url`'s. Account notifications can be polled by continuously following the `next_notification_url`, and handling the resultant notifications, until the `next_notification_url` is an empty string. This means that there are no new notifications but, as soon as there are, querying the same URL that produced this notification in the first place will return the same notification but with the `next_notification_url` set.
        /// 
        /// </summary>
        /// <param name="hash">Notification hash</param>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>Notification instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public Notification GetNotification(string hash, RippleRestClient client)
        {
            var result = client.RestClient.Execute<GetNotificationResponse>(client.CreateGetRequest("v1/accounts/{0}/notifications/{1}", Address, hash));
            client.HandleRestResponseErrors(result);

            result.Data.Notification.Account = this.Address;
            return result.Data.Notification;
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class GetPaymentResponse : RestResponseObject
        {
            [JsonProperty("payment")]
            public Payment Payment { set; get; }
        }

        /// <summary>
        /// Returns an individual payment.
        /// </summary>
        /// <param name="hash">Payment hash or client resource ID</param>
        /// <returns>Payment instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public Payment GetPayment(string hash)
        {
            return GetPayment(hash, RippleRestClient.GetDefaultInstanceOrThrow());
        }

        /// <summary>
        /// Returns an individual payment.
        /// </summary>
        /// <param name="hash">Payment hash or client resource ID</param>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <returns>Payment instance</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public Payment GetPayment(string hash, RippleRestClient client)
        {
            var result = client.RestClient.Execute<GetPaymentResponse>(client.CreateGetRequest("v1/accounts/{0}/payments/{1}", Address, hash));
            client.HandleRestResponseErrors(result);

            return result.Data.Payment;
        }

        [Serializable]
        [TypeConverter(typeof(SerializableExpandableObjectConverter))]
        private class FindPaymentPathsResponse : RestResponseObject
        {
            [JsonProperty("payments")]
            public List<Payment> Payments { set; get; }
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, string destinationAmount)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, string destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, string destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, string destinationAmount, List<string> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, string destinationAmount)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, string destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, string destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, string destinationAmount, List<string> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, Amount destinationAmount)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, Amount destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, Amount destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(Account destinationAccount, Amount destinationAmount, List<string> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, Amount destinationAmount)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, Amount destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, Amount destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(string destinationAccount, Amount destinationAmount, List<string> sourceCurrencies)
        {
            return FindPaymentPaths(RippleRestClient.GetDefaultInstanceOrThrow(), destinationAccount, destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, Amount destinationAmount)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount.ToString());
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, Amount destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount.ToString(), sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, Amount destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount.ToString(), sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, Amount destinationAmount, List<string> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount.ToString(), sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, string destinationAmount)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, string destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, string destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, Account destinationAccount, string destinationAmount, List<string> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount.ToString(), destinationAmount, sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, Amount destinationAmount)
        {
            return FindPaymentPaths(client, destinationAccount, destinationAmount.ToString());
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, Amount destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount, destinationAmount.ToString(), sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, Amount destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount, destinationAmount.ToString(), sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, Amount destinationAmount, List<string> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount, destinationAmount.ToString(), sourceCurrencies);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, string destinationAmount)
        {
            return FindPaymentPaths(client, destinationAccount, destinationAmount, (List<string>) null);
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, string destinationAmount, List<Amount> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount, destinationAmount, sourceCurrencies == null ? null : sourceCurrencies.ConvertAll((o) => o.ToCurrencyString()));
        }

        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, string destinationAmount, List<Balance> sourceCurrencies)
        {
            return FindPaymentPaths(client, destinationAccount, destinationAmount, sourceCurrencies == null ? null : sourceCurrencies.ConvertAll((o) => o.ToCurrencyString()));
        }


        /// <summary>
        /// Query `rippled` for possible payment "paths" through the Ripple Network to deliver the given amount to the specified `destination_account`. If the `destination_amount` issuer is not specified, paths will be returned for all of the issuers from whom the `destination_account` accepts the given currency.
        /// </summary>
        /// <param name="client">A RippleRestClient used for this request.</param>
        /// <param name="destinationAccount">Destination account</param>
        /// <param name="destinationAmount">Destination amount</param>
        /// <param name="sourceCurrencies">an array of source currencies that can be used to constrain the results returned (e.g. `["XRP", "USD+r...", "BTC+r..."]`) Currencies can be denoted by their currency code (e.g. USD) or by their currency code and issuer (e.g. `USD+r...`). If no issuer is specified for a currency other than XRP, the results will be limited to the specified currencies but any issuer for that currency will do.</param>
        /// <returns>Payment instances</returns>
        /// <exception cref="RippleRestException">Request failed.</exception>
        public List<Payment> FindPaymentPaths(RippleRestClient client, string destinationAccount, string destinationAmount, List<string> sourceCurrencies)
        {
            var srcCury = "";
            if (sourceCurrencies != null && sourceCurrencies.Count > 0)
            {
                srcCury = "?" + String.Join(",", sourceCurrencies);
            }
            var request = client.CreateGetRequest("v1/accounts/{0}/payments/paths/{1}/{2}{3}", Address, destinationAccount, destinationAmount, srcCury);
            var result = client.RestClient.Execute<FindPaymentPathsResponse>(request);
            client.HandleRestResponseErrors(result);

            return result.Data.Payments;
        }

    }
}

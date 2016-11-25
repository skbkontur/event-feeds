﻿using Humanizer;

namespace SKBKontur.Catalogue.Core.EventFeed.Recipes
{
    public static class ElasticsearchIdentifiersConversionExtensions
    {
        /// <summary>
        ///     CamelCaseIdentified -> camel-case-identified
        /// </summary>
        public static string CamelCaseForElasticsearch(this string value)
        {
            return value.Underscore().Dasherize();
        }
    }
}
<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <NuGetReference>YamlDotNet</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>YamlDotNet.RepresentationModel</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var client = new HttpClient();
    using (var response = await client.GetAsync("https://raw.githubusercontent.com/github/linguist/master/lib/linguist/languages.yml"))
    {
        response.EnsureSuccessStatusCode();
        var sb = new StringBuilder();
        sb.AppendLine(@"/*!
 * Language Badge
 * Copyright 2018      Mikazuki
 * Copyright 2011-2018 The Bootstrap Authors
 * Copyright 2011-2018 Twitter, Inc.
 * Licensed under MIT (https://github.com/mika-f/language-badge/blob/master/LICENSE)
 */
.badge {
  display: inline-block;
  padding: .25em .4em;
  font-size: 75%;
  font-weight: 700;
  line-height: 1;
  text-align: center;
  white-space: nowrap;
  vertical-align: baseline;
  border-radius: .25rem;
}");
        sb.AppendLine();
        using (var strategy = await response.Content.ReadAsStreamAsync())
        {
            using (var reader = new StreamReader(strategy))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                var root = yaml.Documents[0].RootNode as YamlMappingNode;
                foreach (var entry in root.Children)
                {
                    var key = (entry.Key as YamlScalarNode);
                    var value = (entry.Value as YamlMappingNode);
                    if (!value.Children.ContainsKey("color"))
                    {
                        continue;
                    }
                    sb.AppendLine($".badge-{key.Value.ToLower().Replace(" ", "-").Replace("'", "").Replace("+", "plus").Replace("#", "sharp")} {{");
                    var foreground = "fff";
                    if (ColorTranslator.FromHtml((value.Children["color"] as YamlScalarNode).Value).GetIntensities() > 186)
                    {
                        foreground = "000";
                    }
                    sb.AppendLine($"  color: #{foreground};");
                    sb.AppendLine($"  background-color: {value.Children["color"]};");
                    sb.AppendLine("}");
                    sb.AppendLine();
                    // entry.Key.Dump();
                }
            }
        }
        using (var sw = new StreamWriter(@".\language-badge.css"))
        {
            sw.WriteLine(sb.ToString());
        }
    }
}

// Define other methods and classes here
public static class ColorExtension
{
    public static double GetIntensities(this Color color)
    {
        return color.R * 0.299 + color.G * 0.587 + color.B * 0.114;
    }
}
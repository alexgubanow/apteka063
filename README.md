# apteka063
tg bot for local pharmacy service

## Localization

The translated text should be added as ID to the `Resources/Translation.resx` via the Microsoft Visual Studio ... do not forget to save.

After saving the `Resources/Translation.Designer.cs` will be updated automatically. So this id could be used on code like 
```
Resources.Translation.NewTextIDForTranslation
```

The key part is to update the `Culture Info` before using the translatable id like the following
```
Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(languageCode);
```

`languageCode` is either `uk` for Ukrainian and `ru` for Russian languages. Everything else will go to English.
This code could be easily obtained from each Message or CallBack using `From` attribute.

For transaltion to another langauges you just need to add the IDs from `Resources/Translation.resx` to all `Resources/Translation.??.resx` files which represents corresponding langauge

set TOOL="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools"
SET RES=MyResources

cd %1\Properties
%TOOL%\resgen %RES%.resx /str:c#,BatInspector.Properties,%RES%,%RES%.Designer.cs /publicClass


using Volo.Abp.TextTemplating;

namespace EventHub.Emailing
{
    public class EmailTemplateDefinitionProvider : TemplateDefinitionProvider
    {
        public override void Define(ITemplateDefinitionContext context)
        {
            context.Add(
                new TemplateDefinition(
                    EmailTemplates.EventRegistration
                ).WithVirtualFilePath(
                    "/Emailing/Templates/EventRegistration.tpl",
                    isInlineLocalized: true
                )
            );
            
            context.Add(
                new TemplateDefinition(
                    EmailTemplates.EventReminder
                ).WithVirtualFilePath(
                    "/Emailing/Templates/EventReminder.tpl",
                    isInlineLocalized: true
                )
            );
            
            context.Add(
                new TemplateDefinition(
                    EmailTemplates.NewEventCreated
                ).WithVirtualFilePath(
                    "/Emailing/Templates/NewEventCreated.tpl",
                    isInlineLocalized: true
                )
            );

            context.Add(
                new TemplateDefinition(
                        EmailTemplates.EventTimingChanged)
                    .WithVirtualFilePath(
                        "/Emailing/Templates/EventTimingChanged.tpl",
                        isInlineLocalized: true
                    )
            );
        }
    }
}
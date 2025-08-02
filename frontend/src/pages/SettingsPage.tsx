import { useState } from 'react';
import { useTheme } from '@/components/ThemeProvider';
import { useAuth } from '@/lib/auth/AuthContext';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Separator } from '@/components/ui/separator';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { useToast } from '@/hooks/use-toast';
import {
  Settings as SettingsIcon,
  User,
  Palette,
  Bell,
  Shield,
  Save,
  Moon,
  Sun,
} from 'lucide-react';

export default function SettingsPage() {
  const { user } = useAuth();
  const { theme, setTheme } = useTheme();
  const { toast } = useToast();

  // Form states
  const [profileData, setProfileData] = useState({
    name: user?.name || '',
    surname: user?.surname || '',
    email: user?.email || '',
  });

  const [preferences, setPreferences] = useState({
    emailNotifications: true,
    pushNotifications: false,
    darkMode: theme === 'dark',
    autoSave: true,
  });

  const handleProfileSave = () => {
    // In a real app, this would call an API
    toast({
      title: 'Profile Updated',
      description: 'Your profile has been successfully updated.',
    });
  };

  const handlePreferencesSave = () => {
    // Update theme if changed
    if (preferences.darkMode !== (theme === 'dark')) {
      setTheme(preferences.darkMode ? 'dark' : 'light');
    }

    toast({
      title: 'Preferences Saved',
      description: 'Your preferences have been successfully saved.',
    });
  };

  const getUserInitials = (name?: string, surname?: string) => {
    if (!name && !surname) return 'U';
    return `${name?.charAt(0) || ''}${surname?.charAt(0) || ''}`.toUpperCase();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Settings</h1>
          <p className="text-muted-foreground">
            Manage your account settings and preferences
          </p>
        </div>
        <SettingsIcon className="h-8 w-8 text-muted-foreground" />
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Profile Settings */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              Profile Information
            </CardTitle>
            <CardDescription>
              Update your personal information and account details
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Avatar Section */}
            <div className="flex items-center gap-4">
              <Avatar className="h-16 w-16">
                <AvatarFallback className="bg-gradient-to-r from-blue-600 to-purple-600 text-white text-lg">
                  {getUserInitials(profileData.name, profileData.surname)}
                </AvatarFallback>
              </Avatar>
              <div>
                <p className="font-medium">
                  {profileData.name} {profileData.surname}
                </p>
                <p className="text-sm text-muted-foreground">
                  {profileData.email}
                </p>
                <Badge variant="secondary" className="mt-1">
                  {user?.role || 'User'}
                </Badge>
              </div>
            </div>

            <Separator />

            {/* Form Fields */}
            <div className="grid gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="firstName">First Name</Label>
                  <Input
                    id="firstName"
                    value={profileData.name}
                    onChange={e =>
                      setProfileData(prev => ({
                        ...prev,
                        name: e.target.value,
                      }))
                    }
                    placeholder="Enter your first name"
                  />
                </div>
                <div>
                  <Label htmlFor="lastName">Last Name</Label>
                  <Input
                    id="lastName"
                    value={profileData.surname}
                    onChange={e =>
                      setProfileData(prev => ({
                        ...prev,
                        surname: e.target.value,
                      }))
                    }
                    placeholder="Enter your last name"
                  />
                </div>
              </div>

              <div>
                <Label htmlFor="email">Email Address</Label>
                <Input
                  id="email"
                  type="email"
                  value={profileData.email}
                  onChange={e =>
                    setProfileData(prev => ({ ...prev, email: e.target.value }))
                  }
                  placeholder="Enter your email"
                />
              </div>
            </div>

            <Button onClick={handleProfileSave} className="w-full">
              <Save className="mr-2 h-4 w-4" />
              Save Profile
            </Button>
          </CardContent>
        </Card>

        {/* Preferences */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Palette className="h-5 w-5" />
              Preferences
            </CardTitle>
            <CardDescription>
              Customize your application experience
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Theme Settings */}
            <div>
              <h4 className="font-medium mb-3 flex items-center gap-2">
                {theme === 'dark' ? (
                  <Moon className="h-4 w-4" />
                ) : (
                  <Sun className="h-4 w-4" />
                )}
                Appearance
              </h4>
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <div>
                    <Label htmlFor="darkMode">Dark Mode</Label>
                    <p className="text-sm text-muted-foreground">
                      Use dark theme across the application
                    </p>
                  </div>
                  <Switch
                    id="darkMode"
                    checked={preferences.darkMode}
                    onCheckedChange={checked =>
                      setPreferences(prev => ({ ...prev, darkMode: checked }))
                    }
                  />
                </div>
              </div>
            </div>

            <Separator />

            {/* Notifications */}
            <div>
              <h4 className="font-medium mb-3 flex items-center gap-2">
                <Bell className="h-4 w-4" />
                Notifications
              </h4>
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <div>
                    <Label htmlFor="emailNotifications">
                      Email Notifications
                    </Label>
                    <p className="text-sm text-muted-foreground">
                      Receive notifications via email
                    </p>
                  </div>
                  <Switch
                    id="emailNotifications"
                    checked={preferences.emailNotifications}
                    onCheckedChange={checked =>
                      setPreferences(prev => ({
                        ...prev,
                        emailNotifications: checked,
                      }))
                    }
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <Label htmlFor="pushNotifications">
                      Push Notifications
                    </Label>
                    <p className="text-sm text-muted-foreground">
                      Receive push notifications in browser
                    </p>
                  </div>
                  <Switch
                    id="pushNotifications"
                    checked={preferences.pushNotifications}
                    onCheckedChange={checked =>
                      setPreferences(prev => ({
                        ...prev,
                        pushNotifications: checked,
                      }))
                    }
                  />
                </div>
              </div>
            </div>

            <Separator />

            {/* Other Settings */}
            <div>
              <h4 className="font-medium mb-3 flex items-center gap-2">
                <Shield className="h-4 w-4" />
                Application
              </h4>
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <div>
                    <Label htmlFor="autoSave">Auto Save</Label>
                    <p className="text-sm text-muted-foreground">
                      Automatically save changes
                    </p>
                  </div>
                  <Switch
                    id="autoSave"
                    checked={preferences.autoSave}
                    onCheckedChange={checked =>
                      setPreferences(prev => ({ ...prev, autoSave: checked }))
                    }
                  />
                </div>
              </div>
            </div>

            <Button onClick={handlePreferencesSave} className="w-full">
              <Save className="mr-2 h-4 w-4" />
              Save Preferences
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* System Information */}
      <Card>
        <CardHeader>
          <CardTitle>System Information</CardTitle>
          <CardDescription>Application and account details</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <Label className="text-sm font-medium">Application Version</Label>
              <p className="text-sm text-muted-foreground">v1.0.0</p>
            </div>
            <div>
              <Label className="text-sm font-medium">Last Login</Label>
              <p className="text-sm text-muted-foreground">
                {new Date().toLocaleDateString()}
              </p>
            </div>
            <div>
              <Label className="text-sm font-medium">Account Type</Label>
              <p className="text-sm text-muted-foreground">
                {user?.roles?.length ? user.roles.join(', ') : 'Standard'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

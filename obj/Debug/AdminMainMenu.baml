   M S B A M L   `   `   `  ����C  ?CityNest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:CityNestCityNest  5   	       CityNest.AdminMainMenu   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:CityNest     5   	     � Title$   Администратор��5   	   $	��560��6   $
��1200��6,    � WindowStartupLocation$ CenterScreen=�5	   	    � WindowStyle$
 None=�5
   	   
������  � AllowsTransparency . .��5      �� 
���   � �� CornerRadius$ 20��6    ._�5   
   �  � ColumnDefinitions 5      �� 5      $	��254 �6#   �� 5      .+�5      �� $��0��6   ���#201D2C$��2��6>   $��	0,0,944,0q�6R   .+�5      ��  �� Orientation$ 
Horizontal=�6   $��60��66   ���#201D2C$��0 0 0 30q�6W   .+�5      �� $��CityNest��6    
�������$��22(�6C   $
��Bold"�6Q   $��Center=�6c   $��10 0 0 0q�6~   5      �� 5      -   5        $��Отчёты��5         
MenuButton#��[  6*   
 � Tag$ Reports��6N   $	��235��6u   �� 5      -   5        $'��!Настройки Профиля��5      #��[  65   $ Settings��6Y   $	��235��6�   �� 5      ���#3E3B4F$��10 20q�61   �� 5      -   5        $D��>Регистрация новых 
пользователей��5      #��[  6M   $ RegistrationForm��6q   $��70��6�   $	��236��6�   �� 5      -   5        $7��1Управление пользователями��5      #��[  6=   $ UserManagement��6a   $��34��6�   $	��235��6�   �� 5!      ���#3E3B4F$��10 20q�61   �� 5#      -   5        $��
Выйти��5#      #��[  6)   $ Logout��6M   $��41��6s   $	��235��6   5$      � 5'      $��1��6   $��74,20,20,20q�6#   .+�5(      r� -   5         r�Name$ BackgroundVideo��5(      	 r� Source$	 Folder/admin.mp4?�5)      
 r� Stretch$
 UniformToFill=�5*       r� LoadedBehavior$ Manual=�5+       r� UnloadedBehavior$
 Stop=�5,       r� IsMuted . $y�Visible=�5/       J� Opacity$	 0.3Q�50      $��42,0,0,0q�51       � 52      -   5          �Name$ ContentFrame��52        � NavigationUIVisibility$ Hidden=�53      
������ $��
47,-17,0,0q�55      56      57      58      59      